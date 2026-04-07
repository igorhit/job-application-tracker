# Notas de Implementação — Application Tracker

Este documento registra problemas reais encontrados durante o desenvolvimento, como cada um foi solucionado e por que a solução escolhida foi a mais adequada.

O objetivo é tornar o processo de desenvolvimento transparente: qualquer desenvolvedor experiente sabe que esses problemas são esperados e a forma de resolvê-los diz mais sobre a qualidade do trabalho do que a ausência deles.

---

## 1. JWT secret lido na hora errada: `ConfigureAppConfiguration` vs. tempo de registro de serviços

**Problema**

O `DependencyInjection.AddInfrastructure` lia `configuration["Jwt:Secret"]` e jogava `InvalidOperationException` se o valor fosse nulo — o que sempre acontecia nos testes de integração, mesmo com o valor sendo injetado corretamente pelo `WebApplicationFactory`.

**O que causava**

O `WebApplicationFactory` injeta configuração de teste via `ConfigureAppConfiguration`. Esse callback é adicionado ao pipeline do host builder e executado antes de `ConfigureServices`. Porém, o `Program.cs` chama `builder.Services.AddInfrastructure(builder.Configuration)` usando o `IConfiguration` que já foi construído — um snapshot do estado de configuração no momento em que a linha é executada. O snapshot não contém os valores do `ConfigureAppConfiguration` porque eles ainda não foram aplicados naquele ponto do ciclo de vida.

O resultado: a linha `configuration["Jwt:Secret"] ?? throw new InvalidOperationException(...)` sempre jogava exceção nos testes, porque o snapshot usado era o da aplicação real, sem os overrides de teste.

**Como foi solucionado**

A configuração do JWT Bearer foi separada em duas etapas:

1. Registro do serviço de autenticação (sem parâmetros ainda):
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
```

2. Configuração dos parâmetros via `IPostConfigureOptions`, que é resolvida pelo container em runtime, quando toda a configuração já está disponível:
```csharp
services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new PostConfigureOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured");
        options.TokenValidationParameters = new TokenValidationParameters { ... };
    });
});
```

**Por que essa solução**

`IPostConfigureOptions` é executado quando o container resolve o serviço pela primeira vez — depois de toda a configuração ter sido construída, incluindo os overrides do `ConfigureAppConfiguration`. Isso garante que tanto a aplicação real (que carrega o `.env` via `EnvBootstrap`) quanto os testes (que injetam configuração via factory) recebem os valores corretos.

A alternativa seria fazer o `WebApplicationFactory` injetar as configurações via variáveis de ambiente reais, mas isso polui o processo de teste e cria dependência de estado global.

---

## 2. `WebApplicationFactory` tentando criar o arquivo `.env` durante os testes

**Problema**

O `EnvBootstrap` era chamado no início de `Program.cs` antes de qualquer verificação de ambiente. Durante os testes de integração, cada classe de teste criava sua própria instância de `WebApplicationFactory`, e todas tentavam criar o mesmo arquivo `.env` no diretório de saída dos testes simultaneamente.

**O que causava**

`System.IO.IOException: The process cannot access the file because it is being used by another process`. O erro acontecia porque duas factories tentavam abrir o mesmo arquivo para escrita ao mesmo tempo, já que os testes de suítes diferentes são executados em paralelo pelo xUnit.

**Como foi solucionado**

O bootstrap passou a ser condicional ao ambiente, usando `builder.Environment.EnvironmentName` — que é definido pelo `UseEnvironment("Testing")` da factory antes de qualquer outra coisa:

```csharp
var isTesting = builder.Environment.EnvironmentName == "Testing";

if (!isTesting)
{
    EnvBootstrap.EnsureEnvFileExists(appRoot);
    EnvBootstrap.LoadEnvFile(appRoot);
    builder.Configuration.AddEnvironmentVariables();
}
```

**Por que essa solução**

Usar `builder.Environment.EnvironmentName` em vez de `Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")` é importante: o `UseEnvironment("Testing")` da factory modifica o ambiente do host builder, não a variável de ambiente do processo. Ler a variável diretamente retornaria `"Development"` (ou o valor real do ambiente) mesmo em contexto de teste.

---

## 3. SQLite em memória causando erros de tabela não encontrada nos testes

**Problema**

A primeira versão da `IntegrationTestFactory` usava SQLite em memória (`Data Source=:memory:`). Os testes falhavam com erros como `no such table: Users` mesmo depois de chamar `EnsureCreatedAsync()`.

**O que causava**

SQLite em memória cria um banco por conexão. O `WebApplicationFactory` usa injeção de dependência com `DbContext` com escopo de request: cada request HTTP abre e fecha um `DbContext`, o que abre e fecha uma conexão SQLite. Quando a conexão é fechada, o banco em memória desaparece. O `EnsureCreatedAsync()` cria as tabelas em uma conexão, mas os requests dos testes operam em conexões diferentes — cada uma com seu próprio banco vazio.

**Como foi solucionado**

O banco de testes foi trocado para SQLite em arquivo temporário com nome único:

```csharp
private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
```

O arquivo persiste durante toda a execução da factory e é removido no `DisposeAsync`.

**Por que essa solução**

SQLite em arquivo mantém os dados entre conexões diferentes, o que é o comportamento correto para os testes de integração. O `Guid.NewGuid()` no nome garante que cada instância de factory use seu próprio arquivo, isolando as suítes de teste umas das outras sem depender de qualquer infraestrutura externa.

---

## 4. Rate limiting quebrando testes de integração

**Problema**

A suíte de integração executava múltiplos requests contra os endpoints de auth em sequência rápida. Os primeiros testes passavam, mas testes posteriores começavam a receber `429 Too Many Requests` de forma não determinística.

**O que causava**

O `AspNetCoreRateLimit` usa contadores em memória que não são resetados entre testes. A regra `POST /auth/register → 5 por hora` era esgotada rapidamente durante a execução da suíte, porque todos os testes de registro compartilhavam os mesmos contadores na mesma instância da factory.

**Como foi solucionado**

O registro do rate limiting foi colocado dentro do bloco `if (!isTesting)`:

```csharp
if (!isTesting)
{
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(...);
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
}
```

**Por que essa solução**

Rate limiting é proteção de infraestrutura de produção. Testá-lo nos testes de integração comuns criaria testes frágeis e dependentes de ordem de execução. Se fosse necessário testar rate limiting especificamente, isso deveria ser feito em testes isolados com configuração controlada. Para os testes de domínio, o comportamento correto é não haver limite.

---

## 5. Cliente HTTP com `BaseAddress` errado nos testes de integração

**Problema**

Um teste de `CompanyTests` criava um `new HttpClient { BaseAddress = _client.BaseAddress }` diretamente, em vez de usar o cliente fornecido pela factory. O teste falhava com `System.Net.Sockets.SocketException: No connection could be made because the target machine actively refused it (localhost:80)`.

**O que causava**

O `HttpClient` criado manualmente é um cliente HTTP real que tenta abrir uma conexão TCP. `_client.BaseAddress` no contexto de teste aponta para o servidor de teste em memória, mas um cliente criado fora da factory não sabe como se conectar a esse servidor — ele tenta conectar a `localhost:80` ou o endereço literal, que não está escutando.

A `WebApplicationFactory` cria clientes especiais que se comunicam diretamente com o `TestServer` em memória, sem passar pela rede. Um `new HttpClient()` comum não tem esse comportamento.

**Como foi solucionado**

O teste foi refatorado para criar todos os clientes via `_factory.CreateClient()`:

```csharp
// antes — cliente manual, tenta conexão TCP real
using var client = new HttpClient { BaseAddress = _client.BaseAddress };

// depois — cliente gerenciado pela factory, usa TestServer em memória
var client = _factory.CreateClient();
```

**Por que essa solução**

É a forma correta de criar clientes no contexto de `WebApplicationFactory`. O `CreateClient()` retorna um `HttpClient` pré-configurado para se comunicar com o `TestServer` interno, sem abrir sockets reais. Todos os clientes usados em testes de integração devem ser criados por esse método.

---

## 7. Sobrescrita de arquivo bloqueada pela tool de escrita sem leitura prévia

**Problema**

Durante o desenvolvimento do frontend, a primeira escrita de `src/frontend/src/lib/api.ts` criou uma versão incompleta do arquivo (sem os exports `getAccessToken` e `tryRefreshOnLoad`). Ao tentar reescrever o arquivo com a versão completa, a operação foi bloqueada com erro: `File has not been read yet`.

**O que causava**

A ferramenta de edição de arquivos exige que o arquivo seja lido antes de ser sobrescrito, como salvaguarda contra destruição acidental de conteúdo existente. A primeira escrita bem-sucedida criou o arquivo; a tentativa de reescrever foi recusada porque o arquivo existente não havia sido lido na sessão atual após a criação.

**Como foi solucionado**

Ler o arquivo criado (mesmo que brevemente, apenas para satisfazer a verificação) antes de fazer a sobrescrita. Em seguida, a ferramenta de escrita aceitou o novo conteúdo completo.

**Por que essa solução**

A salvaguarda existe por boa razão — evita perda de trabalho. O fluxo correto ao sobrescrever um arquivo existente é sempre: ler primeiro, depois escrever. Mesmo que o conteúdo atual seja descartável, a leitura prévia confirma intenção consciente de sobrescrita.

---

## 6. Arquivo `.db` de teste bloqueado no `DisposeAsync`

**Problema**

Após os testes, o `DisposeAsync` da factory tentava deletar o arquivo `.db` temporário e lançava `System.IO.IOException: The process cannot access the file because it is being used by another process`.

**O que causava**

O SQLite mantém um lock no arquivo enquanto há conexões abertas. Mesmo depois de `await base.DisposeAsync()` completar, o garbage collector do .NET pode não ter finalizado todos os objetos que seguravam conexões SQLite, especialmente em cenários de execução rápida de testes.

**Como foi solucionado**

A deleção foi encapsulada em um método que ignora erros silenciosamente:

```csharp
private static void TryDelete(string path)
{
    try { if (File.Exists(path)) File.Delete(path); }
    catch { /* best effort */ }
}
```

**Por que essa solução**

Arquivos temporários em `Path.GetTempPath()` são limpos pelo sistema operacional periodicamente. Não conseguir deletar o arquivo imediatamente é inofensivo — o teste já terminou, o banco estava isolado por nome único, e o OS cuidará da limpeza. Propagar a exceção causaria falsos negativos: testes que passaram seriam reportados como falhos por um problema de teardown não relacionado ao que estava sendo testado.

---

## 8. `project.assets.json` com caminhos Windows copiado para o container Linux

**Problema**

O `docker compose up --build` falhava no estágio `dotnet publish` com:

```text
NuGet.Packaging.Core.PackagingException: Unable to find fallback package folder
'C:\Program Files (x86)\Microsoft Visual Studio\Shared\NuGetPackages'
```

**O que causava**

O `COPY . .` no Dockerfile copiava os diretórios `obj/` locais para dentro do container. Esses diretórios contêm o `project.assets.json` gerado pelo `dotnet restore` na máquina Windows, com caminhos absolutos como `C:\Program Files\...`. O `dotnet publish --no-restore` usava esses assets corrompidos em vez de gerar novos.

**Como foi solucionado**

Criado `.dockerignore` em `src/backend/` excluindo `**/bin/` e `**/obj/`. Com isso, o `COPY . .` não copia os assets locais, e o `dotnet restore` dentro do container gera assets com caminhos Linux corretos.

**Por que essa solução**

`.dockerignore` é o mecanismo padrão para excluir arquivos do build context do Docker — análogo ao `.gitignore`. Excluir `obj/` e `bin/` é boa prática em qualquer Dockerfile .NET, pois evita que artefatos de build locais contaminem o build do container.

---

## 9. `curl` ausente na imagem base `dotnet/aspnet:8.0`

**Problema**

O healthcheck do `docker-compose.yml` usava `curl -f http://localhost:5001/health`, mas o container ficava `unhealthy` com erro:

```text
OCI runtime exec failed: exec: "curl": executable file not found in $PATH
```

**O que causava**

A imagem `mcr.microsoft.com/dotnet/aspnet:8.0` é baseada em Debian slim e não inclui `curl` nem `wget` por padrão — apenas o runtime do .NET.

**Como foi solucionado**

Adicionado `apt-get install -y curl` no Dockerfile do backend, na etapa de runtime:

```dockerfile
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
```

**Por que essa solução**

É a abordagem mais explícita e portável — a dependência fica declarada no Dockerfile, visível para qualquer pessoa que revisar o projeto. A alternativa seria usar um healthcheck baseado em `dotnet` ou um script shell, mas instalar `curl` é mais simples e amplamente utilizado em imagens de produção .NET.

---

## 10. CORS não configurado — frontend bloqueado pelo browser

**Problema**

O login retornava "Erro ao fazer login" no frontend, mesmo com o backend respondendo corretamente. A chamada de `http://localhost:3000` para `http://localhost:5001` era bloqueada pelo browser com erro de CORS.

**O que causava**

O backend não tinha nenhuma configuração de CORS. O browser bloqueia requisições cross-origin (origens diferentes, mesmo que em localhost) quando o servidor não retorna o header `Access-Control-Allow-Origin`. O backend retornava 200 OK para chamadas diretas via `curl`, mas o browser adicionava o header `Origin: http://localhost:3000` e bloqueava a resposta por ausência de CORS.

**Como foi solucionado**

Adicionado `AddCors` e `UseCors` no `Program.cs`, com a origem permitida configurável via variável de ambiente `Cors__AllowedOrigins`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration["Cors:AllowedOrigins"]?.Split(',')
            ?? ["http://localhost:3000"];
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});
// ...
app.UseCors();
```

No `docker-compose.yml`: `Cors__AllowedOrigins=http://localhost:3000`.

**Por que essa solução**

Configurar a origem via variável de ambiente permite que o mesmo container seja usado em diferentes ambientes (desenvolvimento local, staging, produção) sem rebuild. `AllowCredentials()` é necessário porque o frontend envia cookies e headers de autorização. A lista de origens separada por vírgula permite múltiplas origens se necessário.

---

## 11. Playwright usando `127.0.0.1` enquanto o backend aceitava apenas `localhost`

**Problema**

Os testes E2E do Playwright preenchiam o formulário de login corretamente, mas permaneciam em `/login` com a mensagem "Erro ao fazer login", mesmo com a API aceitando o usuário demo por `curl`.

**O que causava**

O `playwright.config.ts` usava `http://127.0.0.1:3000` como `baseURL`, enquanto o backend estava configurado com `Cors__AllowedOrigins=http://localhost:3000`. Para o browser, `127.0.0.1` e `localhost` são origens diferentes. O frontend carregava normalmente, mas as chamadas `fetch` para `http://localhost:5001` eram bloqueadas por CORS porque o header `Origin` enviado pelo browser era `http://127.0.0.1:3000`.

**Como foi solucionado**

O `baseURL` padrão do Playwright foi alinhado com a origem já permitida pelo backend:

```ts
const baseURL = process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:3000'
```

**Por que essa solução**

O problema não estava no fluxo de autenticação nem na API, e sim na origem usada pelo browser automatizado. Ajustar o `baseURL` era a correção mínima e mais precisa, preservando a configuração de CORS já usada pelo `docker compose` e evitando duplicar origens equivalentes apenas para contornar a suíte de testes.

---

## 12. Testes E2E poluindo o dataset demo persistido no volume Docker

**Problema**

Os smoke tests do Playwright criavam empresas e candidaturas usando o próprio usuário demo (`demo@tracker.dev`). Quando uma execução era interrompida ou falhava antes do cleanup final, registros como `Playwright Role ...` permaneciam no banco SQLite persistido no volume Docker e passavam a aparecer para qualquer pessoa que abrisse a aplicação localmente.

**O que causava**

O problema não era a persistência do Docker em si, e sim o acoplamento indevido entre a suíte E2E e o dataset demo. Como o volume `backend_data` preserva o banco entre reinicializações, qualquer dado criado no escopo do usuário demo continuava visível nas próximas execuções.

**Como foi solucionado**

Foi criado um usuário E2E dedicado no seed opcional do backend, habilitado apenas quando configurado no ambiente. A suíte Playwright passou a autenticar com esse usuário e a executar um cleanup prévio de empresas e candidaturas com prefixo `Playwright ` antes de começar o fluxo:

```ts
await cleanupPlaywrightArtifacts(request)
await login(page, E2E_EMAIL, E2E_PASSWORD)
```

Com isso, todo o CRUD exercitado pela suíte fica restrito ao usuário de testes e os resíduos de execuções anteriores são removidos antes de cada rodada, sem contaminar o dashboard e as listagens do usuário demo.

**Por que essa solução**

Essa abordagem preserva o valor do dataset demo para recrutadores e isola os testes de ponta a ponta sem exigir que o usuário saiba remover volumes Docker manualmente. É uma separação de responsabilidades melhor: o ambiente demo continua limpo por padrão, enquanto os E2E continuam cobrindo o fluxo real da aplicação com credenciais estáveis e previsíveis no CI.

---

## 13. Atualização da candidatura quebrando ao substituir requisitos persistidos

**Problema**

Depois de adicionar requisitos estruturados por candidatura, a edição de uma vaga existente passou a falhar com `500` no backend e `Erro ao salvar` no frontend sempre que os requisitos eram reenviados no `PUT /applications/{id}`.

**O que causava**

O aggregate `JobApplication` estava limpando e recriando a coleção `Requirements` diretamente no método `Update()`, enquanto a camada de persistência também tentava sincronizar a entidade principal já rastreada pelo `DbContext`. Na prática, o EF Core acabava emitindo operações conflitantes sobre os mesmos dependentes e lançava `DbUpdateConcurrencyException` ao salvar.

**Como foi solucionado**

A atualização dos campos escalares da candidatura permaneceu no aggregate, mas a substituição da coleção de requisitos foi movida explicitamente para o repositório:

```csharp
application.Update(...);
_applications.ReplaceRequirements(application, request.Requirements);
await _applications.SaveChangesAsync(ct);
```

No repositório, os requisitos antigos são removidos pelo `DbSet<ApplicationRequirement>` e os novos são inseridos novamente com a ordem correta, sem depender de `Clear()` na coleção carregada.

**Por que essa solução**

Coleções dependentes persistidas são um ponto delicado no tracking do EF Core. Deixar a troca completa dessa coleção na infraestrutura torna o comportamento explícito e previsível, enquanto o aggregate continua responsável apenas pelas regras do domínio mais simples. O resultado foi um update estável, coberto por teste de integração e refletido corretamente no fluxo E2E.

---

## 14. Cards de modo de prompt com nome acessível instável quebrando o Playwright

**Problema**

Depois de adicionar as variações de prompt na tela de detalhe da candidatura, o teste E2E falhava ao tentar clicar no modo `Preparação para entrevista`, mesmo com o card visível na interface.

**O que causava**

Cada card era um `<button>` contendo dois textos: o título do modo e a descrição. O nome acessível calculado pelo browser passou a ser a composição desses dois textos, não apenas o título visível. Com isso, o seletor `getByRole('button', { name: 'Preparação para entrevista' })` ficou instável e deixou de corresponder exatamente ao elemento esperado.

**Como foi solucionado**

Os botões dos modos de prompt passaram a declarar explicitamente seu nome acessível e seu estado selecionado:

```tsx
<button
  aria-label={option.label}
  aria-pressed={studyPromptMode === option.value}
>
```

**Por que essa solução**

O ajuste resolve a fragilidade do Playwright sem depender de seletores mais frouxos, melhora a semântica do controle para tecnologias assistivas e deixa explícito qual opção está ativa. É uma correção melhor do que adaptar o teste para navegar por uma árvore de texto incidental do card.

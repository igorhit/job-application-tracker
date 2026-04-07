# Arquitetura — Application Tracker

> Problemas encontrados durante a implementação e como foram resolvidos estão documentados em [implementation-notes.md](implementation-notes.md).

## Objetivo arquitetural

Este projeto demonstra fullstack real, diferentemente do `SecureVaultApi` (que é API pura):

- autenticação e sessão persistente no frontend
- dashboard com métricas ao vivo
- CRUD completo com validações
- integração frontend-backend com tokens JWT
- execução reproduzível via `git clone + docker compose up`

## Arquitetura geral

```text
Next.js (App Router)
        |
        v (HTTP/JSON + JWT Bearer)
.NET 8 Web API
        |
        v
SQLite (arquivo local)
```

---

## Backend

### Clean Architecture

```text
Domain → Application → Infrastructure → API
```

As dependências apontam apenas para dentro: `Infrastructure` implementa interfaces de `Domain`, `Application` define contratos, `API` orquestra.

**Por que:** separação testável de responsabilidades. Os handlers de application não conhecem o banco de dados — apenas interfaces. Os testes unitários substituem implementações reais por mocks sem precisar de banco.

### CQRS com MediatR

Comandos e queries são separados: `CreateApplicationCommand`, `GetAllApplicationsQuery`, etc. O pipeline do MediatR executa `ValidationBehavior` antes de todo handler.

**Por que:** organização explícita de intenção de escrita vs. leitura. O pipeline de validação é automático — nenhum handler precisa chamar validador manualmente. Novo handler = nova classe isolada, sem modificar código existente.

### SQLite embutido

Banco criado automaticamente, migrations aplicadas no startup, dados demo semeados na primeira execução.

**Por que:** zero instalação, zero servidor. O avaliador faz `git clone + docker compose up` e o banco aparece pronto com dados de exemplo. Não há pré-requisito de PostgreSQL ou SQL Server instalado.

### EnvBootstrap

Na primeira execução, o `EnvBootstrap.EnsureEnvFileExists` cria um `.env` com JWT secret gerado aleatoriamente (32 bytes, Base64Url).

**Por que:** o avaliador não precisa criar variáveis de ambiente manualmente. A segurança do JWT não depende de um valor default fraco hardcoded no repositório.

### JWT 15 minutos + refresh 7 dias com rotação

Access token de vida curta, refresh token com rotação a cada renovação (o token anterior é invalidado).

**Por que:** access token de vida curta limita a janela de exploração em caso de vazamento. Rotação do refresh token detecta reutilização suspeita — se um token rotacionado for usado novamente, significa que alguém obteve uma cópia antiga.

### Argon2id para senhas

**Por que:** OWASP-recomendado. Resistente a ataques de GPU e ASIC por consumo de memória configurável. Melhor que bcrypt ou PBKDF2 para o mesmo custo computacional.

### 404 em vez de 403 para recursos de outros usuários

Todos os queries verificam `userId` antes de retornar. Se o recurso não pertence ao usuário autenticado, retorna 404 (não encontrado), não 403 (proibido).

**Por que:** retornar 403 confirma que o recurso existe. Retornar 404 não revela informação sobre a existência de recursos de outros usuários — comportamento de segurança padrão em APIs multi-tenant.

### IPostConfigureOptions para JWT secret

O JWT secret é lido via `IPostConfigureOptions<JwtBearerOptions>`, não diretamente em `AddInfrastructure`.

**Por que:** o `WebApplicationFactory` dos testes injeta configuração via `ConfigureAppConfiguration`, que é executado depois de `ConfigureServices`. Ler o secret em tempo de registro pegaria o valor antes dos overrides de teste. `IPostConfigureOptions` resolve em runtime, quando toda a configuração já está disponível. Detalhes em [implementation-notes.md](implementation-notes.md#1-jwt-secret-lido-na-hora-errada).

---

## Frontend

### Next.js App Router com TypeScript e Tailwind

**Por que Next.js:** App Router com Server/Client Components é o padrão atual de React para produção. Demonstra familiaridade com o ecossistema moderno sem precisar configurar um bundler manualmente.

**Por que Tailwind:** produtividade alta para UI funcional sem biblioteca de componentes. Não exige nenhum servidor de estilos e o bundle final é pequeno (purge automático).

### Token em memória + refresh em localStorage

Access token mantido em uma variável de módulo JS (`let _accessToken`). Refresh token salvo em `localStorage`.

**Por que não cookies HTTP-only:** o backend é uma API separada. Cookies HTTP-only funcionam bem quando o frontend e o backend compartilham domínio/subdomínio. Com domínios separados (localhost:3000 e localhost:5001), configurar `SameSite` e CORS para cookies cross-origin é mais complexo sem ganho real de segurança no contexto de portfólio.

**Por que não localStorage para o access token:** access token em `localStorage` é acessível a qualquer JavaScript da página — incluindo scripts injetados por XSS. Manter em memória elimina esse vetor para o token de acesso.

**Trade-off aceito:** o refresh token em `localStorage` ainda é acessível via JS. O trade-off é: sem `localStorage`, a sessão seria perdida a cada reload de página, o que degradaria muito a UX. Para um portfólio, a UX precisa funcionar bem.

### Restauração de sessão no mount

No `AuthContext`, ao montar o componente, o app chama `tryRefreshOnLoad()`: verifica se há refresh token no `localStorage` e, se houver, faz uma requisição de refresh silenciosa para obter um novo access token.

**Por que:** sem esse mecanismo, o usuário seria deslogado a cada reload de página (porque o access token é perdido quando o JS é recarregado). Com ele, a sessão é restaurada automaticamente enquanto o refresh token for válido.

### Payload do JWT para dados do usuário

O nome e e-mail do usuário são extraídos diretamente do payload do JWT (`atob(token.split('.')[1])`), sem endpoint `/me`.

**Por que:** evita um round-trip extra de rede só para buscar informações já presentes no token. O token já foi validado pelo backend — usar o payload é seguro para exibição de UI.

### Retry automático em 401

O cliente HTTP (`api.ts`) intercepta respostas 401 e tenta renovar o access token antes de rejeitar a requisição.

**Por que:** o access token expira em 15 minutos. Sem retry automático, o usuário receberia erros aleatórios enquanto usa a aplicação. Com retry, a renovação é transparente.

### Route group `(app)` com AuthGuard

Todas as páginas protegidas ficam no grupo `(app)`, que inclui um `AuthGuard` no layout. Se o usuário não estiver autenticado, é redirecionado para `/login`.

**Por que:** centraliza a lógica de proteção de rotas em um único ponto. Adicionar uma nova página protegida não exige nenhuma configuração extra — basta criar o arquivo dentro do grupo.

---

## Docker Compose

### Healthcheck no backend antes do frontend subir

O `docker-compose.yml` define `depends_on: backend: condition: service_healthy`. O frontend só sobe depois que o backend responde `/health` com sucesso.

**Por que:** sem esse controle, o frontend poderia tentar conectar ao backend antes que as migrations tivessem sido aplicadas ou o seed executado, causando erros nas primeiras requisições.

### Volume para persistência do SQLite

O banco SQLite é montado em um volume Docker (`backend_data`). Dados persistem entre reinicializações do container.

**Por que:** sem volume, o banco seria recriado a cada `docker compose up`, perdendo dados inseridos manualmente. O seed é idempotente — não recria o usuário demo se ele já existir.

---

## Trade-offs conscientes

| Decisão | Trade-off aceito |
| --- | --- |
| SQLite em vez de PostgreSQL | Não escala para múltiplos writers — aceitável para portfólio single-user |
| Refresh token em localStorage | Acessível via JS — trade-off de UX vs. segurança em contexto de portfólio |
| Sem testes no frontend | Aumentaria tempo de desenvolvimento sem acréscimo proporcional de demonstração técnica |
| UI simples sem biblioteca de componentes | Menos polida visualmente, mais rápida de implementar e sem lock-in |
| Sem internacionalização | App em português brasileiro — fora do escopo do portfólio |

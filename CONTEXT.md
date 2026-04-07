# Application Tracker — Contexto do Projeto

> **Instrução para agentes IA:** Leia este arquivo no início de toda sessão. Ao final de toda sessão ou ao fazer mudanças significativas, atualize as seções relevantes antes de encerrar.

---

## Desenvolvedor

- Fullstack júnior, foco em backend e cibersegurança (cibersec em fase inicial)
- Stack: .NET 8 C# (backend), Next.js (frontend), SQLite/PostgreSQL, Python (automações)
- Comunicação em português brasileiro, código em inglês

## Objetivo do projeto

Construir uma aplicação fullstack para acompanhamento de candidaturas a vagas, com backend em .NET, frontend em Next.js e experiência de avaliação simples para recrutadores.

O projeto deve complementar o `SecureVaultApi`:

- `SecureVaultApi` prova backend, segurança e arquitetura
- `ApplicationTracker` deve provar fullstack, UX, integração frontend-backend e modelagem de produto

---

## Status atual

**Fase:** publicado no GitHub, validado funcionalmente e em consolidação final para portfólio  
**Repositório:** <https://github.com/igorhit/job-application-tracker>  
**Backend:** implementado com Clean Architecture + CQRS (MediatR) + SQLite  
**Testes unitários:** 6/6 passando  
**Testes de integração:** 18/18 passando  
**Testes E2E:** 2/2 smoke tests Playwright passando localmente  
**CI:** passando no GitHub Actions  
**Frontend:** implementado (Next.js App Router), tema escuro como padrão  
**Docker Compose:** funcional — backend healthy, frontend up  
**Seed demo:** funcional (`demo@tracker.dev` / `Demo1234!`, 4 empresas, 5 candidaturas, 3 notas)

### Último estado de validação

Em `2026-04-07`, o ambiente foi revalidado:

- `dotnet test --no-build` passou novamente (`6` unitários + `11` integração)
- `dotnet test` passou com a nova cobertura (`6` unitários + `14` integração)
- `dotnet test` passou novamente com a nova etapa da feature (`6` unitários + `16` integração)
- `dotnet test` passou novamente com a integração opcional de IA (`6` unitários + `18` integração)
- `npm run build` do frontend passou novamente
- `npm run test:e2e` passou (`2` smoke tests Playwright em Chromium)
- `docker compose up --build -d` subiu com sucesso
- backend containerizado ficou **healthy**
- frontend respondeu em `http://localhost:3000/login`
- login demo funcionou na instância correta do Docker: `demo@tracker.dev` / `Demo1234!`
- fluxo principal da API foi exercitado com sucesso na stack Docker: login → criar empresa → criar candidatura → criar nota → buscar dashboard/listagens → atualizar empresa/candidatura → deletar dados temporários
- fluxo principal no browser foi automatizado com Playwright: login → dashboard → empresas → nova candidatura → detalhe → notas → limpeza dos dados temporários
- filtros e ordenação de candidaturas foram implementados na API e no frontend (`q`, `status`, `companyId`, `sortBy`)
- workflow do GitHub Actions foi atualizada para subir a stack e executar os E2E com Playwright após os testes backend
- a camada de UX foi refinada no frontend com skeletons, empty states mais claros, banners de feedback e confirmações explícitas de remoção
- os E2E foram isolados do usuário demo: a suíte agora usa um usuário dedicado (`e2e@tracker.dev`) e limpeza prévia dos artefatos Playwright, sem poluir o dataset persistido no volume Docker
- a lista de candidaturas agora permite exportar em CSV exatamente o recorte filtrado visível na tela
- cada candidatura agora pode armazenar requisitos principais estruturados
- a tela de detalhe da candidatura agora gera prompts locais com IA a partir do contexto da vaga e dos requisitos salvos, com modos de estudo rápido, preparação para entrevista e plano de revisão
- a geração com IA real agora pode ser habilitada opcionalmente via backend, usando a chave do próprio usuário, com provider concreto OpenAI protegido por interface e fallback completo para o prompt local
- o README foi refinado para avaliação por recrutadores e foi criado um arquivo de copies reutilizáveis para GitHub, currículo e LinkedIn em `docs/portfolio-copy.md`

**Importante:** houve uma divergência de ambiente durante a validação. A porta `5001` estava inicialmente ocupada por um `ApplicationTracker.API.exe` local fora do Docker, e essa instância não refletia o estado esperado do `docker compose`. Após encerrar o processo local e subir a stack correta, o usuário demo funcionou normalmente.

**Observação:** a workflow de CI foi atualizada localmente para incluir o job E2E, mas a execução remota no GitHub Actions ainda depende de push para ser confirmada no repositório.

**Próxima ação sugerida:** o próximo passo mais lógico agora é tirar screenshots, publicar a apresentação final do projeto e seguir para o Projeto 3 do portfólio.

---

## Problemas resolvidos nesta sessão (resumo)

Todos documentados em `docs/implementation-notes.md`:

1. JWT secret lido antes dos overrides de teste → `IPostConfigureOptions`
2. `WebApplicationFactory` criando `.env` em paralelo → guard `if (!isTesting)`
3. SQLite em memória perdendo dados entre conexões → SQLite em arquivo temporário
4. Rate limiting quebrando testes → desativado no ambiente Testing
5. `HttpClient` manual ignorando TestServer → usar `_factory.CreateClient()`
6. Arquivo `.db` bloqueado no Dispose → `TryDelete()` silencioso
7. Sobrescrita de arquivo bloqueada sem leitura prévia → ler antes de escrever
8. `project.assets.json` com caminhos Windows no container → `.dockerignore` excluindo `obj/` e `bin/`
9. `curl` ausente na imagem `dotnet/aspnet:8.0` → `apt-get install curl` no Dockerfile
10. CORS não configurado → `AddCors`/`UseCors` com origem via `Cors__AllowedOrigins`
11. Playwright usando `127.0.0.1` → alinhar `baseURL` com `http://localhost:3000`
12. E2E escrevendo no usuário demo → usar usuário E2E dedicado + cleanup prévio
13. Troca de coleção dependente no update da candidatura → mover a substituição de requisitos para o repositório
14. Card de modo de prompt com nome acessível instável → adicionar `aria-label` explícito e `aria-pressed`

---

## Como rodar

```bash
cd "C:\Users\User\Desktop\Coding\Projects\ApplicationTracker"
docker compose up --build
```

- Frontend: `http://localhost:3000`
- Backend / Swagger: `http://localhost:5001`
- Login demo: `demo@tracker.dev` / `Demo1234!`

```bash
# Testes
cd "C:\Users\User\Desktop\Coding\Projects\ApplicationTracker\src\backend"
dotnet test
```

```bash
# E2E (com docker compose rodando)
cd "C:\Users\User\Desktop\Coding\Projects\ApplicationTracker\src\frontend"
npx playwright install
npm run test:e2e
```

---

## Estrutura

```text
ApplicationTracker/
├── .github/workflows/ci.yml
├── docs/
│   ├── architecture.md
│   ├── security-decisions.md
│   └── implementation-notes.md   # 14 problemas documentados
├── src/
│   ├── backend/
│   │   ├── .dockerignore          # exclui obj/ e bin/ do build context
│   │   ├── ApplicationTracker.Domain/
│   │   ├── ApplicationTracker.Application/
│   │   ├── ApplicationTracker.Infrastructure/
│   │   └── ApplicationTracker.API/
│   └── frontend/                  # Next.js 16, App Router, Tailwind, tema escuro, Playwright
├── tests/backend/
│   ├── ApplicationTracker.UnitTests/       # 6 testes
│   └── ApplicationTracker.IntegrationTests/ # 18 testes
├── .env.example
├── docker-compose.yml
└── README.md
```

---

## Arquivos críticos

| Arquivo | Papel |
| --- | --- |
| `src/backend/ApplicationTracker.API/Program.cs` | Entry point: EnvBootstrap, CORS, migrations, middlewares, Swagger |
| `src/backend/ApplicationTracker.API/Infrastructure/EnvBootstrap.cs` | Gera `.env` com JWT secret seguro se não existir |
| `src/backend/ApplicationTracker.Infrastructure/DependencyInjection.cs` | SQLite, repositórios, Argon2, JWT (leitura lazy via IPostConfigureOptions) |
| `src/backend/Dockerfile` | Multi-stage, instala curl, cria appuser, chown /app |
| `src/frontend/src/app/layout.tsx` | Classe `dark` fixada no `<html>` |
| `src/frontend/src/lib/api.ts` | Cliente HTTP com token em memória e retry em 401 |
| `src/frontend/src/lib/csv.ts` | Geração e download do CSV a partir da lista filtrada de candidaturas |
| `src/frontend/src/lib/requirements.ts` | Normalização dos requisitos digitados no frontend |
| `src/frontend/src/lib/studyPrompt.ts` | Geração local de prompts de estudo, entrevista e revisão a partir da candidatura |
| `src/backend/ApplicationTracker.Infrastructure/Ai/OpenAiTextGenerationService.cs` | Cliente opcional da IA real via OpenAI Responses API |
| `src/backend/ApplicationTracker.API/Controllers/AiController.cs` | Endpoint autenticado de status da integração opcional de IA |
| `src/frontend/src/app/(app)/applications/page.tsx` | Lista com busca, filtros por status/empresa e ordenação |
| `src/frontend/src/app/(app)/applications/new/page.tsx` | Formulário com captura de requisitos da vaga |
| `src/frontend/src/app/(app)/applications/[id]/page.tsx` | Detalhe da candidatura com requisitos, prompt local e geração opcional com IA |
| `src/frontend/src/components/FeedbackBanner.tsx` | Banner reutilizável de feedback para sucesso, erro e informações |
| `src/frontend/src/components/ConfirmDialog.tsx` | Modal reutilizável para confirmações destrutivas |
| `src/frontend/src/contexts/AuthContext.tsx` | Restauração de sessão via refresh token no mount |
| `src/frontend/playwright.config.ts` | Configuração dos testes E2E do frontend |
| `src/frontend/tests/e2e/app.spec.ts` | Smoke suite Playwright cobrindo login, navegação e CRUD principal |
| `docker-compose.yml` | `Cors__AllowedOrigins=http://localhost:3000`, healthcheck com curl |
| `tests/backend/ApplicationTracker.IntegrationTests/IntegrationTestFactory.cs` | SQLite temporário em arquivo, env "Testing" |

---

## Regra de documentação de problemas

Todo problema não-trivial encontrado durante o desenvolvimento deve ser registrado **imediatamente** em `docs/implementation-notes.md` no mesmo commit da solução, com o formato:

- **Nome do problema** — título curto e descritivo
- **O que causava** — causa raiz técnica, não apenas o sintoma
- **Como foi solucionado** — solução aplicada, com trecho de código se relevante
- **Por que essa solução** — motivação da decisão

**Esta regra é obrigatória e não depende de solicitação do desenvolvedor.**

---

## Decisões arquiteturais

- **SQLite** embutido, zero instalação
- **EnvBootstrap** gera JWT secret automaticamente na primeira execução
- **Clean Architecture** Domain → Application → Infrastructure → API
- **CQRS com MediatR** + ValidationBehavior pipeline
- **Argon2id** para hash de senhas
- **JWT 15min + refresh 7 dias** com rotação
- **IPostConfigureOptions** para leitura lazy do JWT secret
- **404 em vez de 403** para recursos de outro usuário
- **Token em memória** no frontend (access token), refresh token no localStorage
- **Tema escuro fixo** via classe `dark` no `<html>` (Tailwind v4)
- **CORS** configurável via variável de ambiente `Cors__AllowedOrigins`

---

## Tarefas pendentes

- [x] Implementar backend completo
- [x] Implementar frontend completo
- [x] Docker Compose funcional
- [x] Seed demo
- [x] CI passando
- [x] Publicar no GitHub
- [x] Tema escuro no frontend
- [x] Validar fluxo completo no browser (login → dashboard → candidaturas → empresas → notas)
- [x] Corrigir problemas funcionais encontrados na validação do fluxo automatizado
- [x] Adicionar smoke tests E2E com Playwright
- [x] Integrar Playwright ao GitHub Actions
- [x] Implementar filtros e ordenação em candidaturas
- [x] Melhorar UX (loading, empty states, feedbacks, confirmações)
- [x] Adicionar exportação CSV de candidaturas
- [x] Adicionar requisitos estruturados por candidatura
- [x] Gerar prompt de estudo rápido a partir da vaga
- [x] Adicionar variações de prompt por objetivo (estudo, entrevista, revisão)
- [x] Adicionar integração opcional com IA real usando chave própria do usuário
- [x] Revisar README como peça de portfólio
- [x] Preparar textos curtos para GitHub, currículo e LinkedIn

---

## Portfólio completo (contexto maior)

Este é o **Projeto 2 de 3**:

| # | Projeto | Stack | Status |
| --- | --- | --- | --- |
| 1 | SecureVault API | .NET 8 + SQLite | Concluído e publicado |
| 2 | Application Tracker | .NET + Next.js + SQLite | Publicado, validado, em melhorias |
| 3 | Ferramenta de segurança | Python | Aguardando evolução em cibersec |

Todos os projetos vivem em `C:\Users\User\Desktop\Coding\Projects\` e seguem os requisitos definidos em `C:\Users\User\Desktop\Coding\Projects\CLAUDE.md`.

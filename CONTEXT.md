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

**Fase:** publicado no GitHub, em validação funcional com docker compose  
**Repositório:** <https://github.com/igorhit/job-application-tracker>  
**Backend:** implementado com Clean Architecture + CQRS (MediatR) + SQLite  
**Testes unitários:** 6/6 passando  
**Testes de integração:** 11/11 passando  
**CI:** passando no GitHub Actions  
**Frontend:** implementado (Next.js App Router), tema escuro como padrão  
**Docker Compose:** funcional — backend healthy, frontend up  
**Seed demo:** funcional (`demo@tracker.dev` / `Demo1234!`, 4 empresas, 5 candidaturas, 3 notas)

### Último estado de validação

O `docker compose up --build` está funcionando. O login com as credenciais demo foi testado (backend responde corretamente via curl). O frontend carrega em `http://localhost:3000`. Tema escuro foi aplicado e buildou com sucesso.

**O que ainda não foi validado visualmente pelo desenvolvedor:** login completo via browser + navegação entre páginas. A sessão de IA encerrou antes da confirmação visual final.

**Próxima ação sugerida:** validar o fluxo completo no browser (login → dashboard → candidaturas → empresas → notas) e corrigir eventuais problemas visuais ou funcionais encontrados.

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

---

## Estrutura

```text
ApplicationTracker/
├── .github/workflows/ci.yml
├── docs/
│   ├── architecture.md
│   ├── security-decisions.md
│   └── implementation-notes.md   # 10 problemas documentados
├── src/
│   ├── backend/
│   │   ├── .dockerignore          # exclui obj/ e bin/ do build context
│   │   ├── ApplicationTracker.Domain/
│   │   ├── ApplicationTracker.Application/
│   │   ├── ApplicationTracker.Infrastructure/
│   │   └── ApplicationTracker.API/
│   └── frontend/                  # Next.js 16, App Router, Tailwind, tema escuro
├── tests/backend/
│   ├── ApplicationTracker.UnitTests/       # 6 testes
│   └── ApplicationTracker.IntegrationTests/ # 11 testes
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
| `src/frontend/src/contexts/AuthContext.tsx` | Restauração de sessão via refresh token no mount |
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
- [ ] Validar fluxo completo no browser (login → dashboard → candidaturas → empresas → notas)
- [ ] Corrigir eventuais problemas visuais ou funcionais encontrados na validação

---

## Portfólio completo (contexto maior)

Este é o **Projeto 2 de 3**:

| # | Projeto | Stack | Status |
| --- | --- | --- | --- |
| 1 | SecureVault API | .NET 8 + SQLite | Concluído e publicado |
| 2 | Application Tracker | .NET + Next.js + SQLite | Publicado, em validação final |
| 3 | Ferramenta de segurança | Python | Aguardando evolução em cibersec |

Todos os projetos vivem em `C:\Users\User\Desktop\Coding\Projects\` e seguem os requisitos definidos em `C:\Users\User\Desktop\Coding\Projects\CLAUDE.md`.

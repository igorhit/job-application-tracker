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

**Fase:** implementação concluída, pendente publicação no GitHub  
**Backend:** implementado com Clean Architecture + CQRS (MediatR) + SQLite  
**Build:** compilando, 0 erros  
**Testes unitários:** 6/6 passando  
**Testes de integração:** 11/11 passando  
**Frontend:** implementado (Next.js App Router, token em memória, todas as páginas funcionais)  
**Docker Compose:** configurado com backend + frontend + healthcheck + volume  
**Seed demo:** implementado (usuário `demo@tracker.dev` / Demo1234!, 4 empresas, 5 candidaturas, 3 notas)  
**GitHub:** não publicado ainda  
**CI:** arquivo `.github/workflows/ci.yml` criado, não validado remotamente

**Próxima ação:** inicializar git, push para GitHub, validar CI

---

## Como rodar

```bash
cd "C:\Users\User\Desktop\Coding\Projects\ApplicationTracker\src\backend\ApplicationTracker.API"
dotnet run
```

- Porta: `http://localhost:5001`
- `.env` gerado automaticamente com JWT secret seguro na primeira execução
- Banco SQLite `applicationtracker.db` criado automaticamente
- Migrations aplicadas automaticamente
- Swagger em `http://localhost:5001`

```bash
# Rodar todos os testes
cd "C:\Users\User\Desktop\Coding\Projects\ApplicationTracker\src\backend"
dotnet test
```

---

## Estrutura

```
ApplicationTracker/
├── .github/
│   └── workflows/ci.yml
├── docs/
│   ├── architecture.md
│   └── security-decisions.md
├── src/
│   ├── backend/
│   │   ├── ApplicationTracker.Domain/         # Entidades, interfaces, DomainErrors, Enums
│   │   ├── ApplicationTracker.Application/    # CQRS handlers, validators, DTOs, ValidationBehavior
│   │   ├── ApplicationTracker.Infrastructure/ # EF Core + SQLite, repositórios, Argon2, JWT
│   │   └── ApplicationTracker.API/            # Controllers, Program.cs, EnvBootstrap
│   └── frontend/                              # Next.js (não iniciado)
├── tests/
│   └── backend/
│       ├── ApplicationTracker.UnitTests/      # 6 testes (NSubstitute + FluentAssertions)
│       └── ApplicationTracker.IntegrationTests/ # 11 testes (SQLite temporário em arquivo)
├── .env.example
├── .gitignore
├── CONTEXT.md
├── docker-compose.yml
└── README.md
```

---

## Arquivos críticos

| Arquivo | Papel |
|---|---|
| `src/backend/ApplicationTracker.API/Program.cs` | Entry point: EnvBootstrap, migrations, middlewares, Swagger |
| `src/backend/ApplicationTracker.API/Infrastructure/EnvBootstrap.cs` | Gera `.env` com JWT secret seguro se não existir |
| `src/backend/ApplicationTracker.Infrastructure/DependencyInjection.cs` | SQLite, repositórios, Argon2, JWT (leitura lazy via IPostConfigureOptions) |
| `src/backend/ApplicationTracker.API/appsettings.json` | SQLite connection string, JWT issuer/audience, rate limiting |
| `src/backend/ApplicationTracker.Infrastructure/Persistence/Migrations/` | Migration SQLite gerada |
| `tests/backend/ApplicationTracker.IntegrationTests/IntegrationTestFactory.cs` | SQLite temporário em arquivo, env "Testing" |

---

## Regra de documentação de problemas

Todo problema não-trivial encontrado durante o desenvolvimento deve ser registrado em `docs/implementation-notes.md` com o formato:

- **Nome do problema** — título curto e descritivo
- **O que causava** — causa raiz técnica, não apenas o sintoma
- **Como foi solucionado** — solução aplicada, com trecho de código se relevante
- **Por que essa solução** — motivação da decisão

Este arquivo já existe e contém os problemas resolvidos até agora. Novos problemas devem ser adicionados ao longo do desenvolvimento.

---

## Decisões arquiteturais

- **SQLite** embutido, zero instalação — mesmo padrão do SecureVaultApi
- **EnvBootstrap** gera JWT secret automaticamente na primeira execução
- **Clean Architecture** Domain → Application → Infrastructure → API
- **CQRS com MediatR** + ValidationBehavior pipeline
- **Argon2id** para hash de senhas
- **JWT 15min + refresh 7 dias** com rotação
- **IPostConfigureOptions<JwtBearerOptions>** para leitura lazy do JWT secret (permite overrides nos testes)
- **404 em vez de 403** para recursos de outro usuário (não revela existência)
- **Testes de integração com SQLite temporário** sem Docker

---

## Endpoints implementados

```
POST   /auth/register
POST   /auth/login
POST   /auth/refresh
POST   /auth/logout     [Authorize]

GET    /companies       [Authorize]
POST   /companies       [Authorize]
PUT    /companies/{id}  [Authorize]
DELETE /companies/{id}  [Authorize]

GET    /applications                          [Authorize]
GET    /applications/search?q=               [Authorize]
GET    /applications/{id}                    [Authorize]
POST   /applications                         [Authorize]
PUT    /applications/{id}                    [Authorize]
DELETE /applications/{id}                   [Authorize]

GET    /applications/{id}/notes              [Authorize]
POST   /applications/{id}/notes              [Authorize]
DELETE /applications/{applicationId}/notes/{id} [Authorize]

GET    /dashboard       [Authorize]

GET    /health
```

---

## Pacotes instalados

**Domain:** FluentResults 3.15.2

**Application:** MediatR 12.2.0, FluentValidation 11.9.0, FluentValidation.DI 11.9.0, FluentResults 3.15.2

**Infrastructure:** EF Core 8.0.0, EF Core SQLite 8.0.0, EF Core Design 8.0.0, Konscious.Security.Cryptography.Argon2 1.3.1, Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0

**API:** Swashbuckle.AspNetCore 6.5.0, Serilog.AspNetCore 8.0.0, Serilog.Sinks.Console 5.0.1, AspNetCoreRateLimit 5.0.0, EF Core Design 8.0.0, Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 8.0.0

**UnitTests:** xUnit, FluentAssertions 6.12.0, NSubstitute 5.1.0

**IntegrationTests:** xUnit, Microsoft.AspNetCore.Mvc.Testing 8.0.0, EF Core SQLite 8.0.0, FluentAssertions 6.12.0

---

## Tarefas pendentes

- [x] Definir proposta do Projeto 2
- [x] Criar scaffold inicial do repositório local
- [x] Criar `CONTEXT.md`
- [x] Criar `README.md`
- [x] Criar `docs/architecture.md`
- [x] Criar `docs/security-decisions.md`
- [x] Criar `.env.example`
- [x] Criar `docker-compose.yml` inicial
- [x] Criar estrutura concreta de `src/backend`
- [x] Implementar autenticação no backend
- [x] Implementar entidades `Company`, `JobApplication` e `ApplicationNote`
- [x] Implementar dashboard
- [x] Adicionar testes (6 unitários + 11 integração)
- [x] Criar GitHub Actions CI
- [x] Validar `dotnet run` manualmente no Swagger
- [x] Implementar frontend com Next.js
- [x] Adicionar seed demo
- [x] Atualizar docker-compose.yml com serviços reais
- [ ] Publicar no GitHub

---

## Portfólio completo (contexto maior)

Este é o **Projeto 2 de 3**:

| # | Projeto | Stack | Status |
|---|---|---|---|
| 1 | SecureVault API | .NET 8 + SQLite | Concluído e publicado |
| 2 | Application Tracker | .NET + Next.js + SQLite | Implementação concluída, pendente publicação |
| 3 | Ferramenta de segurança | Python | Aguardando evolução em cibersec |

Todos os projetos vivem em `C:\Users\User\Desktop\Coding\Projects\` e seguem os mesmos requisitos de portfólio definidos em `C:\Users\User\Desktop\Coding\Projects\CLAUDE.md`.

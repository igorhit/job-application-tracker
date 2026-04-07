# Application Tracker

[![CI](https://github.com/igorhit/job-application-tracker/actions/workflows/ci.yml/badge.svg)](https://github.com/igorhit/job-application-tracker/actions/workflows/ci.yml)

Aplicação fullstack para organizar candidaturas a vagas de emprego. Backend em .NET 8, frontend em Next.js, banco SQLite embutido. Funciona com `git clone` + um comando.

## O que resolve

Durante processos seletivos, informações ficam espalhadas entre planilhas, e-mails e notas. Esta aplicação centraliza:

- acompanhamento de candidaturas com status
- dashboard com métricas por etapa do processo
- empresas cadastradas com histórico de vagas
- notas por candidatura (entrevistas, impressões, follow-ups)
- próximas ações com datas

## Por que foi construído assim

- **SQLite embutido**: zero instalação, zero servidor. O banco é criado automaticamente na primeira execução
- **EnvBootstrap**: JWT secret gerado automaticamente — o recrutador não precisa configurar nada
- **Clean Architecture no backend**: demonstra organização, separação de responsabilidades e testabilidade
- **Next.js App Router**: demonstra frontend com autenticação, contexto de sessão e consumo de API real
- Detalhes em [docs/architecture.md](docs/architecture.md)

## Como rodar

### Docker Compose (recomendado)

```bash
git clone <repo>
cd application-tracker
docker compose up --build
```

Aguarde o build (alguns minutos na primeira vez) e acesse:

- **Frontend:** <http://localhost:3000>
- **Backend / Swagger:** <http://localhost:5001>

**Login demo disponível automaticamente:**

```text
E-mail:  demo@tracker.dev
Senha:   Demo1234!
```

### Desenvolvimento local

Pré-requisitos: .NET 8 SDK e Node.js 20+

**Backend:**

```bash
cd src/backend/ApplicationTracker.API
dotnet run
# Swagger: http://localhost:5001
```

**Frontend:**

```bash
cd src/frontend
cp .env.local.example .env.local
npm install
npm run dev
# http://localhost:3000
```

## Como executar os testes

```bash
cd src/backend
dotnet test
```

- **6 testes unitários** (NSubstitute + FluentAssertions)
- **11 testes de integração** (SQLite temporário, zero Docker)

## Variáveis de ambiente

O backend gera automaticamente um `.env` com valores seguros na primeira execução. Nenhuma configuração manual é necessária para rodar localmente.

Para Docker, as variáveis estão definidas no `docker-compose.yml`.

Referência: [`.env.example`](.env.example)

## Decisões arquiteturais

| Decisão | Motivação |
| --- | --- |
| SQLite | Zero instalação, zero servidor — fundamental para `clone + um comando` |
| EnvBootstrap | JWT secret gerado automaticamente, sem configuração manual |
| Clean Architecture | Domain → Application → Infrastructure → API, dependências só apontam para dentro |
| CQRS com MediatR | Commands e queries separados, pipeline de validação automático |
| JWT 15min + refresh 7 dias | Segurança razoável com boa UX, rotação a cada renovação |
| Argon2id | Hash de senhas OWASP-recomendado, resistente a GPU |
| 404 em vez de 403 | Não revela a existência de recursos de outros usuários |

Detalhes: [docs/architecture.md](docs/architecture.md) | [docs/security-decisions.md](docs/security-decisions.md)

## Estrutura

```text
ApplicationTracker/
├── .github/workflows/ci.yml     # CI: build + testes
├── docs/
│   ├── architecture.md          # Por que cada decisão foi tomada
│   ├── security-decisions.md    # Decisões de segurança
│   └── implementation-notes.md  # Problemas encontrados e como foram resolvidos
├── src/
│   ├── backend/                 # .NET 8 — Domain, Application, Infrastructure, API
│   └── frontend/                # Next.js 16 — App Router, TypeScript, Tailwind
├── tests/backend/               # Unitários e de integração
├── .env.example
├── docker-compose.yml
└── README.md
```

## Endpoints da API

```text
POST   /auth/register
POST   /auth/login
POST   /auth/refresh
POST   /auth/logout

GET    /companies
POST   /companies
PUT    /companies/{id}
DELETE /companies/{id}

GET    /applications
GET    /applications/search?q=
GET    /applications/{id}
POST   /applications
PUT    /applications/{id}
DELETE /applications/{id}

GET    /applications/{id}/notes
POST   /applications/{id}/notes
DELETE /applications/{applicationId}/notes/{id}

GET    /dashboard
GET    /health
```

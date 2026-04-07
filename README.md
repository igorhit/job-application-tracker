# Application Tracker

[![CI](https://github.com/igorhit/job-application-tracker/actions/workflows/ci.yml/badge.svg)](https://github.com/igorhit/job-application-tracker/actions/workflows/ci.yml)

Aplicação fullstack para organizar candidaturas a vagas de emprego, construída para demonstrar backend em .NET 8, frontend em Next.js, integração ponta a ponta, UX prática e automação de testes. O projeto roda com SQLite embutido e `docker compose up --build`, sem dependências externas obrigatórias.

## Resumo rápido

- backend em **.NET 8** com **Clean Architecture**, **CQRS**, **MediatR** e **SQLite**
- frontend em **Next.js 16** com autenticação, dashboard, CRUD completo e UX refinada
- **Docker Compose** funcional com seed demo e login pronto para avaliação
- **testes unitários, integração e E2E** com CI no GitHub Actions
- feature diferenciada de **prompt de estudo com IA**, com fallback local e integração real opcional via backend

## Para recrutadores

Este projeto foi pensado para ser avaliado rapidamente:

- suba a stack com `docker compose up --build`
- entre com `demo@tracker.dev` / `Demo1234!`
- teste o fluxo de dashboard, candidaturas, empresas, notas e exportação CSV
- se quiser avaliar engenharia, rode `dotnet test` e `npm run test:e2e`

## O que resolve

Durante processos seletivos, informações ficam espalhadas entre planilhas, e-mails e notas. Esta aplicação centraliza:

- acompanhamento de candidaturas com status
- busca, filtros e ordenação na lista de candidaturas
- dashboard com métricas por etapa do processo
- empresas cadastradas com histórico de vagas
- notas por candidatura (entrevistas, impressões, follow-ups)
- próximas ações com datas
- feedbacks visuais, estados vazios e confirmações de remoção nas telas principais
- exportação CSV da lista filtrada de candidaturas
- requisitos principais estruturados por candidatura
- geração local de prompts para uso em agentes IA, com modos de estudo rápido, preparação para entrevista e plano de revisão
- integração opcional com IA real via backend, usando a própria chave do usuário sem expor segredo no frontend

## Por que foi construído assim

- **SQLite embutido**: zero instalação, zero servidor. O banco é criado automaticamente na primeira execução
- **EnvBootstrap**: JWT secret gerado automaticamente — o recrutador não precisa configurar nada
- **Clean Architecture no backend**: demonstra organização, separação de responsabilidades e testabilidade
- **Next.js App Router**: demonstra frontend com autenticação, contexto de sessão e consumo de API real
- **IA opcional com chave própria**: demonstra integração externa segura sem transformar o projeto em custo obrigatório
- Detalhes em [docs/architecture.md](docs/architecture.md)

## O que este projeto demonstra

- modelagem de produto além de CRUD básico
- integração real frontend-backend com autenticação e estado de sessão
- cuidado com experiência de avaliação para recrutador
- documentação de problemas reais de desenvolvimento em [docs/implementation-notes.md](docs/implementation-notes.md)
- preocupação com segurança prática: Argon2id, JWT com rotação, CORS configurável e segredo fora do frontend

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
- **18 testes de integração** (SQLite temporário, zero Docker)

### Testes E2E do frontend

Com a stack Docker já em execução:

```bash
cd src/frontend
npx playwright install
npm run test:e2e
```

- **2 smoke tests E2E** com Playwright em Chromium
- Cobrem login, navegação principal, empresas, candidaturas e notas
- A exportação CSV da lista filtrada também está coberta no fluxo automatizado
- A criação de requisitos por vaga e a geração dos prompts de estudo/entrevista também entram no fluxo automatizado
- A UI também valida o estado padrão da IA opcional desabilitada, sem exigir chave para rodar a suite
- Os E2E usam um usuário dedicado de teste e limpam artefatos Playwright antes da execução, sem alterar o dataset demo persistido no volume Docker
- A workflow do GitHub Actions também foi preparada para executar esses E2E após subir a stack com `docker compose`

## IA opcional com chave própria

O projeto continua funcionando integralmente sem nenhuma API externa. Se você quiser habilitar geração real de conteúdo com IA, a integração é opcional e roda pelo backend.

Por padrão, o fluxo continua assim:

- o usuário pode gerar o prompt localmente
- pode copiar e colar em qualquer agente IA externo
- nenhum custo adicional é assumido pelo projeto

Para habilitar a integração real, configure no backend:

```env
Ai__Enabled=true
Ai__Provider=OpenAI
Ai__Model=gpt-5-mini
Ai__ApiKey=sua-chave-aqui
Ai__BaseUrl=https://api.openai.com/v1/
Ai__TimeoutSeconds=45
```

Regras importantes:

- a chave fica apenas no backend
- o frontend nunca recebe a chave
- se a IA estiver desabilitada, a aplicação continua funcional com o prompt local
- a implementação atual usa OpenAI via interface abstrata, permitindo adicionar outros providers depois sem refatorar a UI

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

## Sugestão de screenshots para o repositório

Se quiser melhorar ainda mais a apresentação no GitHub, estas são as capturas com maior retorno:

1. dashboard com métricas e próximas ações
2. lista de candidaturas com filtros, ordenação e exportação CSV
3. detalhe da candidatura com requisitos e prompt de estudo
4. tela mostrando o estado da IA opcional desabilitada ou habilitada
5. listagem de empresas

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
POST   /applications/{id}/study-assistant

GET    /applications/{id}/notes
POST   /applications/{id}/notes
DELETE /applications/{applicationId}/notes/{id}

GET    /ai/status
GET    /dashboard
GET    /health
```

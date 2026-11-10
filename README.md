# Credit Analysis System

Sistema de análise de crédito assíncrona com .NET 8, PostgreSQL e RabbitMQ.

## Como funciona

```
Cliente ──POST──▶ API ──salva (Pending)──▶ PostgreSQL
                   │                          ▲
                   └──publica──▶ RabbitMQ     │
                                    │         │
                                    ▼         │
                                  Worker ──avalia e salva (Approved / Rejected)

Cliente ──GET───▶ API ──lê──▶ PostgreSQL
```

1. A API recebe a análise, salva como **Pending** e publica o Id na fila (`202 Accepted`).
2. O worker consome a fila, aplica a política de crédito e salva o resultado.
3. O cliente consulta o resultado pelo `GET`.

### Política de crédito

| Regra | Resultado |
|---|---|
| Score ≥ 700 **e** renda ≥ R$ 2.000,00 | `Approved` |
| Score < 700 | `Rejected` – "Score abaixo do mínimo exigido" |
| Renda < R$ 2.000,00 | `Rejected` – "Renda abaixo do mínimo exigido" |

Os valores ficam em `Domain/Policies/CreditPolicy.cs`.

### Confiabilidade

- **Fila durável e mensagens persistentes**, com *publisher confirms*.
- **Dead letter queue** (`credit-analysis.requested.dlq`) para mensagens que falharam.
- **Rotina de recuperação** no worker: análises presas em `Pending` (RabbitMQ fora, erro no processamento...) são avaliadas automaticamente (padrão: a cada 60s, as que estão há mais de 120s em `Pending`).
- **Idempotência e concorrência otimista** (coluna `xmin` do Postgres): mensagens duplicadas ou atualizações simultâneas não geram resultado inconsistente.

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/credit-analyses` | Cria e envia para avaliação |
| `GET` | `/api/credit-analyses?status=Approved` | Lista (filtro de status opcional) |
| `GET` | `/api/credit-analyses/{id}` | Consulta uma análise |
| `PUT` | `/api/credit-analyses/{id}` | Atualiza e reenvia para avaliação |
| `DELETE` | `/api/credit-analyses/{id}` | Remove (somente análises já avaliadas; pendentes retornam `409`) |
| `GET` | `/health` | Health check (banco de dados) |

Erros seguem o padrão **ProblemDetails** (RFC 7807). Exemplos de requisições em `src/CreditAnalysisSystem.Api/CreditAnalysisSystem.Api.http`.

## Estrutura

```
.
├── CreditAnalysisSystem.sln
├── Directory.Build.props            # configurações comuns dos projetos
├── Directory.Packages.props         # versões dos pacotes NuGet centralizadas
├── src/
│   ├── CreditAnalysisSystem.Domain/         # entidade, status, política de crédito (sem dependências)
│   ├── CreditAnalysisSystem.Application/    # casos de uso, DTOs, interfaces, mensagem da fila
│   ├── CreditAnalysisSystem.Infrastructure/ # EF Core + migrations, repositório, RabbitMQ
│   ├── CreditAnalysisSystem.Api/            # controllers, tratamento de erros
│   └── CreditAnalysisSystem.Worker/         # consumer da fila e rotina de recuperação
└── tests/
    └── CreditAnalysisSystem.UnitTests/
```

Dependências: `Api`/`Worker` → `Infrastructure` → `Application` → `Domain`.

## 🚀 Rodando com Docker Compose

```bash
cp .env.example .env
docker compose up --build
```

- **API (Swagger):** http://localhost:5261/swagger
- **RabbitMQ Dashboard:** http://localhost:15672 (guest/guest)
- Parar: `docker compose down` (use `-v` para apagar os dados)

A API aplica as migrations automaticamente ao iniciar.

## 💻 Desenvolvimento local

```bash
docker compose up postgres rabbitmq -d     # Postgres na porta 5433

dotnet run --project src/CreditAnalysisSystem.Api      # http://localhost:5094/swagger
dotnet run --project src/CreditAnalysisSystem.Worker
```

As configurações locais estão em `appsettings.Development.json` de cada projeto.

### Testes

```bash
dotnet test
```

Inclui testes de domínio, de casos de uso e **testes de arquitetura** (`tests/.../Architecture`), que falham se alguma camada violar a regra de dependência da Clean Architecture.

### Migrations

```bash
dotnet tool restore
dotnet ef migrations add NomeDaMigration \
  -p src/CreditAnalysisSystem.Infrastructure \
  -s src/CreditAnalysisSystem.Infrastructure \
  -o Persistence/Migrations
```

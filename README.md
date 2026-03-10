# Account Movements Report

An event-driven pipeline that processes financial transactions and exposes a consolidated movements report per account through a REST API.

The system is composed of two applications:
- **Seed**: a console app that publishes transaction events to RabbitMQ every 10 seconds, simulating an upstream transaction processor.
- **Movements**: an async receiver that consumes those events and persists them as movements in PostgreSQL, plus a REST API that serves a consolidated report per account containing: current balance, total received, total spent, spending by category, and balance by month.

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/)
- [Docker Compose](https://docs.docker.com/compose/install/)

### How to run on Docker

```bash
docker-compose up -d
```

| Service | URL | Purpose |
|---|---|---|
| Movements API | [localhost:9000/movements/swagger](http://localhost:9000/movements/swagger) | REST API + Swagger UI |
| RabbitMQ Management | [localhost:5020](http://localhost:5020) | Broker admin UI |
| Kibana | [localhost:9001](http://localhost:9001) | Log visualization |
| PostgreSQL | localhost:5432 | Database |
| Elasticsearch | localhost:9200 | Search backend |

##### Configuring Kibana index pattern:
   - Access [`localhost:9001/app/management/kibana/indexPatterns`](http://localhost:9001/app/management/kibana/indexPatterns)
   - `Create data view`
   - Type `fluentd-logs` in name
   - `Create data view` button

### Using kubernetes version

[Transactions kubernetes](https://github.com/matheus-oliveira-andrade/transactions-k8s)

### Technologies

- **C# / .NET 8** — both solutions follow Clean Architecture with four layers: `Domain` (entities and interfaces), `Application` (use-case orchestration via **MediatR** CQRS — commands, queries, events), `Infrastructure` (**EF Core** + **Npgsql** ORM, RabbitMQ implementations), and an entry-point project (API or console host).
- **RabbitMQ** — message broker using a Fanout exchange (`processed-transactions`). Published via `RabbitMQ.Client` (Seed) and consumed via **MassTransit** + **MassTransit.Newtonsoft** (Movements) with a 3-retry policy at 60-second intervals.
- **PostgreSQL 15** — relational database for persisting movements, accessed through EF Core migrations applied automatically at startup.
- **Docker / Docker Compose** — multi-stage container builds for each deployable binary; Compose orchestrates all services with resource limits.
- **Serilog** — structured JSON logging to a shared volume, consumed by **Fluentd** for aggregation and forwarded to **Elasticsearch** + **Kibana** for visualization.
- **xUnit + Moq** for unit tests; **FluentAssertions** + `WebApplicationFactory` with the EF Core InMemory provider for integration tests.
- **GitHub Actions** — CI pipeline with two parallel jobs (one per solution) running restore, build, and test on every push to `master`.

### Architecture

![image](https://github.com/matheus-oliveira-andrade/transactions/assets/32457879/1ab7e4cd-bb39-4ff9-bf0a-b5a9e4f57d06)

**Data flow:**

1. `Seed.Console` reads from a local `transactions.json` file and publishes a transaction event to the `processed-transactions` Fanout exchange in RabbitMQ every 10 seconds.
2. `Movements.AsyncReceiver` consumes from the `transaction-consumer-queue` bound to that exchange, dispatches a `TransactionProcessed` event via MediatR, and persists a `Movement` record in PostgreSQL.
3. `Movements.Api` serves `GET /movements/V1/report/{accountId}`, querying PostgreSQL to return the consolidated `MovementReport` (balance, total received/spent, spending by category, and balance by month).

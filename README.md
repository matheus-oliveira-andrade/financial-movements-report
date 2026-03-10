## Transactions

Project to expose, through an API, the report of movements from the accounts. Transactions are created by the `Seed` console application and published to a topic in `RabbitMQ`, then read by the `Movements.AsyncReceiver` application, which processes and saves the data in the `PostgreSQL` database. This data is then exposed through the `Movements.Api` at the `/report/{{accountId}}` endpoint.

### How to run on docker

With docker installed, run

```bash 
docker-compose up -d
```

Movements api docs are exposed in [`localhost:9000/movements/swagger`](localhost:9000/movements/swagger)

Logs of all applications are available in kibana exposed in [`localhost:9001`](localhost:9001)


##### Configuring index pattern:
   - Access [`localhost:9001/app/management/kibana/indexPatterns`](localhost:9001/app/management/kibana/indexPatterns)
   - `Create data view`
   - Type `fluentd-logs` in name
   - `Create data view` button

### Using kubernetes version

[Transactions kubernetes](https://github.com/matheus-oliveira-andrade/transactions-k8s)

### Technologies

- `C#` was used as the language with `.net 8`, following some of the concepts of `clean architecture`. For `unit tests`, `xunit` and `moq` were used.
- `Docker` was used for the application containers with `docker-compose` for multi-containers.
- `PostgreSQL` was chosen as the database.
- `RabbitMQ` was chosen as the message broker.
- `Fluentd` was used for log aggregation, sending the logs to `Elastic Search`.
- `Kibana` was used for log visualization.
- `GitHub Actions` were used for `CI` while the application was being developed, built, and tested on each push.

### Architecture

![image](https://github.com/matheus-oliveira-andrade/transactions/assets/32457879/1ab7e4cd-bb39-4ff9-bf0a-b5a9e4f57d06)


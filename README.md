# Order & Invoice (Idempotent + EDA Demo)

.NET 8 + PostgreSQL + RabbitMQ ile:
- **Idempotent** sipariþ oluþturma (HTTP `Idempotency-Key`),
- **Outbox / Inbox Pattern** ile **Event-Driven** akýþ,
- Worker ile **stok düþme + fatura üretme** senaryosu.

## Mimari

- **OrderService.API**: HTTP API (idempotent create), Outbox’a event yazar.
- **OrderService.Infrastructure**: EF Core, Npgsql, Outbox/Inbox tablolarý, Rabbit Publisher.
- **OrderService.OutboxWorker**: RabbitMQ Consumer. `order.created` eventini tüketir; stok düþer, `Order`u tamamlar, `Invoice` oluþturur, Inbox’a yazar (idempotent consumer).
- **PostgreSQL**: Ýþlemlerin kaynak veritabaný.
- **RabbitMQ**: Event taþýma.

## Hýzlý Baþlangýç (Docker)

> Docker Desktop açýk olmalý.

```bash
# Build + up
docker compose up -d --build
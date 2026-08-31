# Order Processing ERP — Microservices

A reference implementation of an order-processing system built as six independent .NET 8 microservices,
communicating asynchronously over RabbitMQ using event choreography, each with its own CQRS-structured
application layer and its own SQL database.

## Architecture

```
                         ┌──────────────┐
                         │  Client/UI   │
                         └──────┬───────┘
                    synchronous │ HTTP (per service)
        ┌───────────┬───────────┼───────────┬───────────┐
        ▼           ▼           ▼           ▼           ▼
  ┌──────────┐┌──────────┐┌──────────┐┌──────────┐┌──────────┐
  │ Ordering ││Inventory ││ Payments ││ Shipping ││ Catalog  │
  │   .Api   ││   .Api   ││   .Api   ││   .Api   ││   .Api   │
  └────┬─────┘└────┬─────┘└────┬─────┘└────┬─────┘└────┬─────┘
       │            │           │           │           ▲
       │            │  RabbitMQ │           │           │ sync GET
       └───────┬────┴─────┬─────┴─────┬─────┘           │ (price/validity)
               ▼           ▼           ▼                 │
          ┌─────────────────────────────────┐            │
          │      Integration events bus     │────────────┘
          └────────────────┬────────────────┘
                            ▼
                  ┌───────────────────┐
                  │ Notifications      │
                  │ .Worker (consumer  │
                  │ only, no database) │
                  └───────────────────┘

  Each *.Api owns one Azure SQL database (OrderingDb, InventoryDb, PaymentsDb,
  ShippingDb, CatalogDb) — no service reaches into another service's tables.
```

| Service | Responsibility | Talks to the bus? | Owns a database? |
|---|---|---|---|
| **Ordering** | Order lifecycle (Pending → AwaitingPayment → Confirmed → Shipped / Cancelled) | publishes + consumes | OrderingDb |
| **Inventory** | Stock reservation and compensating release | publishes + consumes | InventoryDb |
| **Payments** | Simulated payment authorization | publishes + consumes | PaymentsDb |
| **Shipping** | Shipment creation once an order is confirmed | publishes + consumes | ShippingDb |
| **Catalog** | Product/price lookup, queried synchronously at order time | — | CatalogDb |
| **Notifications** | Fire-and-forget order status notifications | consumes only | — |

## Tech stack

- **.NET 8 / ASP.NET Core** — one Web API project per service (Controllers), plus a Worker Service for Notifications
- **CQRS** — MediatR commands/queries per service, with a shared FluentValidation pipeline behavior
- **RabbitMQ** via **MassTransit** — pub/sub over the default topology (one exchange per message type, one queue per consumer)
- **Transactional outbox** — a shared `OutboxProcessor<TDbContext>` (see `BuildingBlocks/Common/Outbox`) polls each
  service's own database and publishes staged events, so "save the order" and "announce the order" are never split
  across two systems that can fail independently
- **EF Core 8 + Azure SQL** (SQL Server provider) — **database per service**, no cross-service joins
- **Serilog**, **Swagger**, **Polly** (retry policy on the one synchronous call, Ordering → Catalog)

## The saga (event choreography, no orchestrator)

Every message carries `CorrelationId = OrderId`, so the whole chain for one order can be traced across services.

**Happy path**

```
Ordering        : POST /api/orders → validates each line against Catalog (sync HTTP) → saves Order (Pending)
                  + stages OrderCreatedIntegrationEvent in the same transaction (outbox)
Inventory       : consumes OrderCreated → reserves stock for every line → InventoryReservedIntegrationEvent
Ordering        : consumes InventoryReserved → Order → AwaitingPayment
Payments        : consumes InventoryReserved → charges the cached order total → PaymentAuthorizedIntegrationEvent
Ordering        : consumes PaymentAuthorized → Order → Confirmed → OrderConfirmedIntegrationEvent
Shipping        : consumes OrderConfirmed → creates a shipment → OrderShippedIntegrationEvent
Ordering        : consumes OrderShipped → Order → Shipped
Notifications   : consumes OrderCreated / OrderConfirmed / OrderCancelled / OrderShipped throughout → logs a notification
```

**Compensation (no orchestrator, so every failure publishes its own undo)**

```
Inventory       : can't reserve a line → InventoryReservationFailedIntegrationEvent
Payments        : simulated gateway declines (amount > threshold) → PaymentFailedIntegrationEvent
Ordering        : consumes either failure → Order → Cancelled → OrderCancelledIntegrationEvent
Inventory       : consumes OrderCancelled → releases whatever it had reserved for that order (no-op if nothing was reserved)
```

Payments needs the order total but only acts once inventory is reserved, so it keeps its own tiny local copy of
`{OrderId, CustomerId, TotalAmount}` (`PendingOrder`) populated by consuming `OrderCreated` — the standard
"replicate what you need via events" alternative to calling back into Ordering synchronously. Because `OrderCreated`
and `InventoryReserved` travel on independent queues, they can theoretically race; the `InventoryReserved` consumer
in Payments is configured with a short retry policy to absorb that.

## Project layout

```
src/
  BuildingBlocks/
    Common/            Entity<T> base, Result<T>, MediatR ValidationBehavior, the outbox (message, processor, factory)
    EventBus.Contracts/  every integration event shared across services
  Services/
    Ordering/    Ordering.Domain / .Application / .Infrastructure / .Api
    Inventory/   Inventory.Domain / .Application / .Infrastructure / .Api
    Payments/    Payments.Domain / .Application / .Infrastructure / .Api
    Shipping/    Shipping.Domain / .Application / .Infrastructure / .Api
    Catalog/     Catalog.Domain / .Application / .Infrastructure / .Api
    Notifications/ Notifications.Worker (consumers only)
```

Each `*.Application` project follows the same shape: `Common/Interfaces` for the DbContext/gateway abstractions the
domain needs, then one folder per feature under `Orders|Stock|Payments|Shipments|Products/Commands|Queries`, each
file holding the request record, its validator, and its handler together.

## Running it

### Option A — everything in Docker

```bash
docker compose up --build
```

This starts SQL Server, RabbitMQ (management UI at http://localhost:15672, guest/guest), and all six services.
Each API runs its own `dbContext.Database.Migrate()` on startup (fine for a demo; in a real deployment migrations
should run as a separate pipeline step, not on every boot) and seeds three demo products/stock rows with matching
IDs across Catalog and Inventory.

| Service | URL |
|---|---|
| Ordering.Api | http://localhost:5001/swagger |
| Inventory.Api | http://localhost:5002/swagger |
| Payments.Api | http://localhost:5003/swagger |
| Shipping.Api | http://localhost:5004/swagger |
| Catalog.Api | http://localhost:5005/swagger |
| RabbitMQ management | http://localhost:15672 |

### Option B — services on the host, infra in Docker

```bash
docker compose up sqlserver rabbitmq
dotnet run --project src/Services/Catalog/Catalog.Api
dotnet run --project src/Services/Inventory/Inventory.Api
dotnet run --project src/Services/Payments/Payments.Api
dotnet run --project src/Services/Shipping/Shipping.Api
dotnet run --project src/Services/Ordering/Ordering.Api
dotnet run --project src/Services/Notifications/Notifications.Worker
```

`appsettings.Development.json` in each project already points at `localhost` for both SQL Server and RabbitMQ.

## Try the happy path

```bash
# Wireless Mouse's id is seeded identically in Catalog and Inventory
curl -X POST http://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":"9d5e6f1a-0000-0000-0000-000000000001","items":[{"productId":"11111111-1111-1111-1111-111111111111","quantity":2}]}'

# poll the order — it should walk Pending -> AwaitingPayment -> Confirmed -> Shipped over a few seconds
curl http://localhost:5001/api/orders/{orderId}
```

## Try the compensation path

The simulated payment gateway declines any order over `PaymentGateway:DeclineAboveAmount` (5000 by default —
see `Payments.Api/appsettings.json`). Order enough of the $129 USB-C Dock to cross that, and you should see the
order land on `Cancelled` and inventory get released automatically:

```bash
curl -X POST http://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":"9d5e6f1a-0000-0000-0000-000000000001","items":[{"productId":"33333333-3333-3333-3333-333333333333","quantity":50}]}'
```

## Known simplifications (called out on purpose, not missed)

- **No API gateway** — a real deployment would front these with YARP or Azure API Management; omitted here to keep
  the focus on the service-to-service patterns.
- **No auth** — every endpoint is anonymous. Add JWT bearer auth (Azure AD/Entra ID) per service before this goes anywhere real.
- **Idempotency is handler-level, not a generic inbox** — each command handler checks "have I already done this for
  this OrderId?" before acting, which is enough for at-least-once delivery here, but a high-throughput system would
  want a shared processed-message table instead of re-deriving idempotency per handler.
- **Auto-migration on boot** — convenient for `docker compose up`, wrong for production (migrations should be a
  deliberate, separate deploy step).
- **Simulated Payments gateway and Shipping carrier** — swap `SimulatedPaymentGateway` / the tracking-number
  generator in `Shipping.Application` for real integrations without touching anything else, since both sit behind
  an interface.

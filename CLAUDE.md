# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Order Processing ERP: six independent .NET 8 microservices (Ordering, Inventory, Payments, Shipping, Catalog,
Notifications) that communicate over RabbitMQ (via MassTransit) using event choreography — there is no saga
orchestrator. Each service that owns data has its own CQRS-structured application layer (MediatR) and its own
SQL Server / Azure SQL database. Each of the five data-owning services also has a `*.Domain.Tests` xUnit project
(FluentAssertions) covering its domain entities' business rules — see `dotnet test` below. A seventh service,
Identity, provides authentication for the other five APIs — see **Authentication** below.

## Commands

```bash
# restore + build the whole solution
dotnet restore OrderProcessingERP.sln
dotnet build OrderProcessingERP.sln --no-restore

# run all domain unit tests (all *.Domain.Tests projects)
dotnet test OrderProcessingERP.sln --filter "FullyQualifiedName~Domain.Tests"

# run everything (SQL Server, RabbitMQ, all 6 services) in Docker
docker compose up --build

# run infra only, services on the host (appsettings.Development.json points both at localhost)
docker compose up sqlserver rabbitmq
dotnet run --project src/Services/<Service>/<Service>.Api      # e.g. src/Services/Ordering/Ordering.Api
dotnet run --project src/Services/Notifications/Notifications.Worker
```

### EF Core migrations

There are **five separate DbContexts** (Ordering, Inventory, Payments, Shipping, Catalog — Notifications has none),
so every `dotnet ef` command needs `--project` (the service's `*.Infrastructure`), `--startup-project` (the service's
`*.Api`), and `--context` to disambiguate. The `dotnet-ef` global tool must be installed once
(`dotnet tool install --global dotnet-ef`). Example for Ordering, same shape for the other four:

```bash
dotnet ef migrations add <Name> \
  --project src/Services/Ordering/Ordering.Infrastructure/Ordering.Infrastructure.csproj \
  --startup-project src/Services/Ordering/Ordering.Api/Ordering.Api.csproj \
  --output-dir Persistence/Migrations \
  --context OrderingDbContext
```

Migrations apply automatically on startup (`dbContext.Database.Migrate()` in each `Program.cs`) — convenient for
`docker compose up`, but that's a dev-only shortcut; don't rely on it as the deploy story for anything beyond this repo.

Identity.Api has **three** separate DbContexts of its own (all single-project, so `--project` and `--startup-project`
are both `src/Services/Identity/Identity.Api/Identity.Api.csproj`): `ApplicationDbContext` (ASP.NET Core Identity's
user store), `ConfigurationDbContext` and `PersistedGrantDbContext` (Duende IdentityServer's client/scope config and
token/grant storage, from `Duende.IdentityServer.EntityFramework`). Disambiguate with `--context` and give each a
distinct `--output-dir` (e.g. `Migrations/Identity`, `Migrations/Configuration`, `Migrations/PersistedGrant`).

## Architecture

### Layering, repeated identically per service

Every service under `src/Services/<Service>/` (except Notifications) is split into four projects:

- **`.Domain`** — entities inheriting `Common.Entity<TId>`, with private setters and static factory methods
  (`Order.Create(...)`, `StockItem.Create(...)`); business rules live here, not in handlers.
- **`.Application`** — MediatR commands/queries. One file per feature holds the request record, its
  `FluentValidation` validator, and its handler together (e.g.
  `Ordering.Application/Orders/Commands/CreateOrder/CreateOrderCommand.cs`). `Common/Interfaces/I<Service>DbContext.cs`
  is the only thing handlers depend on for persistence — never the concrete `DbContext`.
- **`.Infrastructure`** — the EF Core `DbContext` + `IEntityTypeConfiguration<T>` classes, MassTransit consumers
  under `Messaging/Consumers/`, and the `DependencyInjection.AddXxxInfrastructure(...)` extension method that wires
  DbContext, MassTransit/RabbitMQ, and (Ordering only) the `HttpClient` to Catalog.
- **`.Api`** — `Program.cs` composes `AddXxxApplication()` + `AddXxxInfrastructure()`, plus Controllers.

`BuildingBlocks/Common` and `BuildingBlocks/EventBus.Contracts` are the only things shared across service
boundaries — resist adding more shared code there; each service's `.Domain`/`.Application` should stay independent.

### Event choreography and the transactional outbox

There is no saga orchestrator. Each service reacts to events it consumes and publishes its own; a failure
publishes a compensating event rather than a coordinator rolling things back. The chain (every message carries
`CorrelationId = OrderId`):

```
Ordering: OrderCreated
  -> Inventory: reserves stock -> InventoryReserved | InventoryReservationFailed
       -> Ordering: AwaitingPayment | Cancelled
  -> Payments (on InventoryReserved): charges -> PaymentAuthorized | PaymentFailed
       -> Ordering: Confirmed -> OrderConfirmed | Cancelled -> OrderCancelled
            -> Inventory (on OrderCancelled): releases whatever it reserved (no-op if nothing was reserved)
  -> Shipping (on OrderConfirmed): creates shipment -> OrderShipped
       -> Ordering: Shipped
  -> Notifications: logs a notification on OrderCreated / OrderConfirmed / OrderCancelled / OrderShipped
```

Every write path that needs to publish an event uses the **transactional outbox** in `BuildingBlocks/Common/Outbox`:
a handler adds its entity changes *and* an `OutboxMessage` (via `OutboxMessageFactory.Create(event)`) to the same
`DbContext` and calls `SaveChangesAsync` once, so the state change and the intent to publish commit atomically. A
generic `OutboxProcessor<TDbContext>` background service (registered per service as
`AddHostedService<OutboxProcessor<XxxDbContext>>()`) polls that service's own `OutboxMessages` table every 5s and
publishes pending rows via MassTransit's `IPublishEndpoint`. **When adding a new event-publishing code path, follow
this pattern rather than calling `IPublishEndpoint.Publish` directly from a handler** — direct publish-before-commit
can announce an event for a change that then fails to save.

MassTransit consumers (in each service's `Infrastructure/Messaging/Consumers/`) are intentionally thin: they
translate an `IConsumer<TEvent>.Consume` into a `_sender.Send(new SomeCommand(...))` call and contain no business
logic themselves — that belongs in the Application-layer handler. Idempotency (handling redelivery of the same
event) is done per-handler, not via a generic inbox — e.g. `ReserveStockCommandHandler` checks whether a
`StockReservation` already exists for the `OrderId` before doing anything.

Payments needs the order total but only acts once inventory is reserved, so it keeps a local read-model copy
(`PendingOrder`, populated by consuming `OrderCreated`) instead of calling back into Ordering synchronously. Because
`OrderCreated` and `InventoryReserved` travel on independent queues and can race, the `InventoryReserved` consumer
in Payments has a MassTransit retry policy configured in `Payments.Infrastructure/DependencyInjection.cs` to absorb that.

### The one synchronous call

Ordering calls Catalog over plain HTTP (`ICatalogServiceClient` / `CatalogServiceClient`, with a Polly retry policy)
at order-creation time to validate each line item and snapshot its current price. Every other cross-service
interaction is asynchronous over RabbitMQ.

### Seeded demo data coupling

`Catalog.Infrastructure/Persistence/Configurations/ProductConfiguration.cs` and
`Inventory.Infrastructure/Persistence/Configurations/StockItemConfiguration.cs` seed the same three product GUIDs
(`WirelessMouseId`, `MechanicalKeyboardId`, `UsbCDockId`) into two separate databases via EF `HasData`. If you add
or change a seeded product, update both files — there is no runtime mechanism keeping them in sync (in a real
system this would be a `ProductCreated` event instead).

### Authentication

`src/Services/Identity/Identity.Api` is a single-project service (no `.Domain`/`.Application` split — it has no
business logic of its own, like `Notifications.Worker`) hosting **Duende IdentityServer** plus ASP.NET Core Identity
for its user store, backed by its own `IdentityDb`. It issues `client_credentials` JWTs; every API scope matches a
service name (`ordering.api`, `inventory.api`, `payments.api`, `shipping.api`, `catalog.api`). Seed clients (dev-only,
in `Identity.Api/Data/SeedData.cs`, same "seed on boot" shortcut as everything else here): `m2m.ordering` (scope
`catalog.api`, used by Ordering's outbound HTTP call to Catalog) and `swagger` (all five scopes, for the "Authorize"
button in each service's Swagger UI).

Each of the five API-owning services calls `services.AddJwtBearerAuthentication(configuration, "<service>.api")`
(`BuildingBlocks/Common/Auth/ApiAuthenticationExtensions.cs`) from `Program.cs`, and its top-level controller carries
`[Authorize(Policy = "<service>.api")]`. The `Identity:Authority` config key (plus `Identity:RequireHttpsMetadata:
false`, since everything runs over plain HTTP here) points at the Identity service — `http://identity-api:8080` in
Docker, `http://localhost:5006` in Development. `Notifications.Worker` has no HTTP API and needs none of this.

Duende IdentityServer requires a commercial license for production use beyond its free tier; this repo runs it
unlicensed (`.AddDeveloperSigningCredential()`, a startup log warning) as a dev/demo setup only, matching the same
posture as the plaintext SA password and migrate-on-boot elsewhere in this repo.

### Environment-driven configuration

Each service's `appsettings.json` uses Docker Compose service hostnames (`sqlserver`, `rabbitmq`, `catalog-api`);
`appsettings.Development.json` overrides those to `localhost` for running with `dotnet run` against
`docker compose up sqlserver rabbitmq`. ASP.NET Core services (`*.Api`) select the override via
`ASPNETCORE_ENVIRONMENT`; `Notifications.Worker` is a generic host, so it uses `DOTNET_ENVIRONMENT` instead. Don't
set `ASPNETCORE_ENVIRONMENT=Development` inside `docker-compose.yml` — that would flip container services back to
`localhost` connection strings and break them.

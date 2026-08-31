---
name: csharp-conventions
description: Microsoft's official C# coding conventions (naming, formatting, language usage) applied to this repo's style. Use whenever writing new C# code, editing existing C# files, or reviewing C# for style/convention issues in this solution.
---

# C# Coding Conventions (Microsoft standard, applied to this repo)

Reference: Microsoft's official C# coding conventions
(https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions).
Apply these whenever writing or editing `.cs` files in this solution, and check
for violations when reviewing C# diffs.

## Naming

- **PascalCase**: types (classes, records, structs, enums), interfaces' name body
  (prefixed with `I`, e.g. `IOrderingDbContext`), methods, properties, events,
  public/internal fields, namespaces, constants.
- **camelCase**: local variables, method parameters.
- **`_camelCase`**: private instance fields (e.g. `_sender`, `_dbContext`). This
  repo already follows this — match it in MassTransit consumers and handlers.
- Do not prefix fields or parameters with Hungarian notation (`m_`, `str`, etc.).
- Name types and members with nouns or noun phrases; name methods with verbs or
  verb phrases (`CreateOrder`, `ReserveStock`).
- Avoid abbreviations except well-known ones (`Id`, `Db`, `Api`). Prefer clarity
  over brevity: `CustomerId` not `custId`.

## Language usage

- Use `var` only when the type is obvious from the right-hand side (e.g.
  `var order = Order.Create(...)`); use an explicit type when it isn't
  (`decimal total = CalculateTotal(items)`).
- Use expression-bodied members for single-expression members (`public string
  FullName => $"{First} {Last}";`), not for multi-statement logic.
- Prefer `is`/pattern matching (`if (result is null)`, `if (order is
  { Status: OrderStatus.Confirmed })`) over manual type checks and comparisons.
- Use `nameof(...)` instead of hardcoded strings when referring to a member name
  (validation error messages, logging).
- Use string interpolation (`$"Order {orderId} not found"`) over
  `string.Concat`/`string.Format` for readability.
- Prefer collection initializers and target-typed `new()` where the type is
  already clear from context (`private readonly List<OrderItem> _items = new();`).
- Use `readonly` for fields that are only assigned in the constructor (e.g.
  injected dependencies: `private readonly ISender _sender;`).
- Enable and respect nullable reference types (`#nullable enable` / project-wide
  `<Nullable>enable</Nullable>`) — don't suppress warnings with `!` unless the
  non-null invariant is truly guaranteed and non-obvious enough to deserve a
  one-line comment explaining why.
- Use `async`/`await` all the way up the call stack for I/O-bound work (EF Core
  queries, MassTransit publish/send, HTTP calls); don't block on `.Result` or
  `.Wait()`.

## Formatting

- Allman brace style: opening brace on its own new line, for all constructs
  (`if`, `for`, methods, types).
- One statement per line; one declaration per line.
- Use four-space indentation, not tabs.
- Prefer file-scoped namespaces (`namespace Ordering.Domain;`) over block-scoped
  ones for new files, matching modern .NET 8 style.
- Keep `using` directives outside the namespace, sorted with `System.*` first.

## This repo's established patterns (follow, don't deviate)

- **Domain entities**: private setters, construction only through a static
  factory method (`Order.Create(...)`, `StockItem.Create(...)`) — never a public
  constructor or object initializer for entities.
- **Application layer**: one file per feature holds the MediatR request record,
  its FluentValidation validator, and its handler together, colocated under
  `Feature/Commands/<Verb><Noun>/<Verb><Noun>Command.cs` or
  `Feature/Queries/...`.
- **Persistence access**: handlers depend on the service's
  `I<Service>DbContext` interface, never the concrete `DbContext` class.
- **MassTransit consumers**: stay thin — translate `Consume` into a single
  `_sender.Send(new SomeCommand(...))` call; no business logic in the consumer
  itself.
- **Event publishing**: always via the transactional outbox
  (`OutboxMessageFactory.Create(event)` added to the same `DbContext` and saved
  in one `SaveChangesAsync`) — never call `IPublishEndpoint.Publish` directly
  from a handler.

When these repo-specific patterns and the general Microsoft conventions above
don't conflict, follow both. When reviewing existing code that predates this
skill, don't do a blanket reformat — apply these conventions to code you are
actively adding or touching.

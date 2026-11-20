# ECommercePoc

A monolithic e-commerce API proof-of-concept built with **.NET 8** and **Clean Architecture**, demonstrating CQRS, domain-driven design, and testable layered architecture.

## Architecture

```
src/
├── ECommercePoc.Domain/         Entities, Value Objects, Enums, domain exceptions
├── ECommercePoc.Application/    CQRS (MediatR), DTOs, validators (FluentValidation)
├── ECommercePoc.Infrastructure/ EF Core DbContext, repositories, UnitOfWork, JWT auth
└── ECommercePoc.Api/            ASP.NET Core 8 controllers, middleware, Swagger

tests/
├── ECommercePoc.Domain.Tests/       xUnit — domain logic
├── ECommercePoc.Application.Tests/  xUnit + Moq — handlers & validators
└── ECommercePoc.Api.Tests/          xUnit + WebApplicationFactory — integration tests
```

### Layers

| Layer | Responsibility | Dependencies |
|-------|---------------|-------------|
| **Domain** | `Order` aggregate root, `Money`/`Address` value objects, `OrderStatus` enum, domain exceptions | None |
| **Application** | Command/query handlers via MediatR, DTOs, FluentValidation validators, pipeline behaviors (logging, validation) | Domain |
| **Infrastructure** | EF Core `AppDbContext`, repository implementations, `UnitOfWork`, JWT `TokenService`, `DataSeeder` | Domain, Application |
| **API** | REST controllers (`OrdersController`, `ProductsController`, `AuthController`), global exception middleware, Swagger, JWT auth | Application, Infrastructure |

### Key Design Decisions

- **Order is the aggregate root** — enforces invariants (can't cancel shipped orders)
- **CQRS via MediatR** — commands and queries are separate, each with its own handler
- **Pipeline behaviors** — cross-cutting concerns (logging, validation) decorate handler execution
- **EF Core + SQLite** — zero-config database for development; swappable to any EF Core provider
- **JWT authentication** — token-based auth with a login endpoint
- **API versioning** — `Asp.Versioning` for future-proofing the API surface

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or a newer SDK **plus** the .NET 8 / ASP.NET Core 8 runtime — the projects target `net8.0`, so tests and the API need the 8.0 runtime to run)

## Quick Start

```bash
# Clone
git clone https://github.com/engmsalem/e-commerce-monolitic-poc.git
cd e-commerce-monolitic-poc

# Build
dotnet build

# Run tests
dotnet test

# Start the API (http profile)
dotnet run --project src/ECommercePoc.Api

# ...or with HTTPS
dotnet run --project src/ECommercePoc.Api --launch-profile https
```

The API listens on `http://localhost:5251` (and `https://localhost:7082` with the `https` profile). Swagger UI is at `/swagger` and is only enabled in the `Development` environment, which the launch profiles set.

## Authentication

The POC uses hardcoded credentials (no user store):

| Username | Password |
|----------|----------|
| `admin`  | `admin123` |

```bash
curl -X POST http://localhost:5251/api/v1/auth/login   -H "Content-Type: application/json"   -d '{"username":"admin","password":"admin123"}'
```

The response contains a `token` (valid for 1 hour). Send it on protected endpoints as `Authorization: Bearer <token>`.

## Example: Place an Order

1. Get product IDs from `GET /api/v1/products`.
2. Get the seeded customer ID from the startup log (`Seeded 5 products and 1 customer (ID: ...)`). There's no customer endpoint yet.
3. Create the order:

```bash
curl -X POST http://localhost:5251/api/v1/orders   -H "Authorization: Bearer <token>"   -H "Content-Type: application/json"   -d '{"customerId":"<customer-id>","items":[{"productId":"<product-id>","quantity":2}]}'
```

Errors come back as RFC 7807 `application/problem+json`: `400` for validation failures and business-rule violations, `404` for missing resources, `500` for anything else.

## API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/api/v1/auth/login` | No | Authenticate and get a JWT token |
| `GET` | `/api/v1/products` | No | List all products |
| `POST` | `/api/v1/products` | Yes | Create a product |
| `GET` | `/api/v1/orders/{id}` | Yes | Get an order by ID |
| `POST` | `/api/v1/orders` | Yes | Create a new order |
| `POST` | `/api/v1/orders/{id}/cancel` | Yes | Cancel an order (fails if already shipped or cancelled) |

## Database

Uses SQLite by default (`ECommercePoc.db`, created in the working directory at startup via `EnsureCreated` — there are no EF Core migrations yet). The `DataSeeder` adds 5 sample products and one test customer (Jane Smith) on first run. Delete the `.db` file to reset.

The connection string lives in `ConnectionStrings:Default` in `appsettings.json`.

## Configuration

| Key | Default | Notes |
|-----|---------|-------|
| `ConnectionStrings:Default` | `Data Source=ECommercePoc.db` | SQLite connection string |
| `Jwt:Secret` | POC value in `appsettings.json` | Signing key, at least 32 characters |
| `Jwt:Issuer` | `ECommercePoc` | |
| `Jwt:Audience` | `ECommercePoc.Api` | |

Any key can be overridden with environment variables (e.g. `Jwt__Secret`) or `dotnet user-secrets`.

## Limitations (POC)

This is not production-ready:

- Login credentials are hardcoded, and the JWT secret is committed in `appsettings.json`. Replace both before deploying anywhere.
- No migrations: schema changes require deleting the database.
- Order status only moves from `Pending` to `Cancelled`. There are no endpoints yet to confirm, ship, or deliver orders.
- MediatR 13+ is commercially licensed. Check that its license terms fit your use before using this in production.

## License

Licensed under the [Apache License 2.0](LICENSE).

Copyright 2026 engmsalem

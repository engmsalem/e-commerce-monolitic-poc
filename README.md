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
├── ECommercePoc.Application.Tests/  xUnit + NSubstitute — handlers & validators
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

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Quick Start

```bash
# Clone
git clone https://github.com/engmsalem/e-commerce-monolitic-poc.git
cd e-commerce-monolitic-poc

# Build
dotnet build

# Run tests
dotnet test

# Start the API
dotnet run --project src/ECommercePoc.Api
```

The API starts at `https://localhost:7001` (Swagger UI at `/swagger`).

## API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/api/v1/auth/login` | No | Authenticate and get a JWT token |
| `GET` | `/api/v1/products` | No | List all products |
| `POST` | `/api/v1/products` | Yes | Create a product |
| `GET` | `/api/v1/orders/{id}` | Yes | Get an order by ID |
| `POST` | `/api/v1/orders` | Yes | Create a new order |
| `DELETE` | `/api/v1/orders/{id}` | Yes | Cancel an order |

## Database

Uses SQLite by default (`ECommercePoc.db` created at startup). The `DataSeeder` populates sample products and a test customer on first run.

## License

This is a proof-of-concept project — use freely as a reference or starting point.

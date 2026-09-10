# Multi-Tenant Stores Backend

> A production-oriented multi-tenant commerce backend built with **.NET 10**, **ASP.NET Core**, **DDD**, **CQRS**, **MediatR**, **PostgreSQL**, **Redis**, and **Stripe**.

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet\&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?logo=dotnet\&logoColor=white)](https://learn.microsoft.com/aspnet/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-4169E1?logo=postgresql\&logoColor=white)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7-DC382D?logo=redis\&logoColor=white)](https://redis.io/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker\&logoColor=white)](https://www.docker.com/)

---

## Overview

**Multi-Tenant Stores Backend** is an ASP.NET Core Web API for a multi-store commerce platform where multiple stores can operate on shared backend infrastructure while keeping application responsibilities clearly separated.

The system covers the core customer shopping journey:

**authentication → discovery → product browsing → cart → favorites → checkout → payment processing**

The project is intentionally structured around **Domain-Driven Design and CQRS** rather than treating the backend as a collection of CRUD controllers.

The primary engineering goals are:

* Clear separation of business logic from infrastructure
* Explicit application use cases through commands and queries
* Tenant-aware commerce workflows
* Centralized validation
* Replaceable infrastructure integrations
* Secure authentication and external identity providers
* PostgreSQL-backed persistence
* Redis-backed caching and session/verification workflows
* Containerized local development
* Automated tests across multiple architectural layers
* Continuous verification through GitHub Actions

---

## Architecture

The solution follows a layered architecture with clear dependency boundaries.

```text
┌─────────────────────────────────────────────────────────────┐
│                         Clients                             │
│                  Web / Mobile / External                   │
└─────────────────────────────┬───────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                         API Layer                           │
│        Controllers · Middleware · Auth · OpenAPI           │
└─────────────────────────────┬───────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       Application                           │
│   Commands · Queries · Handlers · DTOs · Validation        │
└───────────────┬─────────────────────────────┬───────────────┘
                │                             │
                ▼                             ▼
┌──────────────────────────┐       ┌──────────────────────────┐
│         Domain           │       │      Infrastructure      │
│                          │       │                          │
│ Entities · Enums         │       │ EF Core / PostgreSQL     │
│ Business Rules           │       │ Redis                    │
│ Domain Contracts         │       │ JWT                      │
└──────────────────────────┘       │ Stripe                   │
                                   │ Google Auth              │
                                   │ SMTP / MailKit           │
                                   └──────────────────────────┘
```

### Request flow

A typical request moves through the system as follows:

```text
HTTP Request
     │
     ▼
Controller
     │
     ▼
MediatR Command / Query
     │
     ▼
Validation Pipeline
     │
     ▼
Application Handler
     │
     ├──────────────► Domain
     │                Business Rules
     │
     └──────────────► Infrastructure
                       │
                       ├── PostgreSQL
                       ├── Redis
                       └── External Services
```

This keeps HTTP concerns out of application logic and prevents infrastructure concerns from leaking into the domain model.

---

## Why DDD + CQRS?

The project uses DDD and CQRS where they provide practical value.

### Domain-Driven Design

The domain layer contains the core business concepts and contracts without depending on infrastructure implementations.

```text
Domain
├── Entities
├── Enums
└── Interfaces
```

This gives business rules a stable place to live while infrastructure details remain replaceable.

### CQRS

Application features are expressed as explicit commands and queries.

```text
Features/
├── Authentication/
├── Customers/
├── Addresses/
├── Catalog/
├── Cart/
├── Favorites/
└── Checkout/
```

Commands represent state-changing operations.

Queries represent read operations.

MediatR is used as the application dispatch mechanism between the API layer and application handlers.

### Validation pipeline

Request validation is centralized through **FluentValidation** and the MediatR pipeline rather than duplicating validation logic across controllers.

---

## Core Features

### Authentication

* Guest sessions
* User registration
* Email verification
* Verification email resend
* Login
* Google authentication
* Access-token refresh
* Logout
* Forgot password
* Password reset

### Customer Management

* Retrieve current customer profile
* Update customer profile
* Change password

### Address Management

* List addresses
* Retrieve an address
* Create an address
* Update an address
* Delete an address
* Set default address

### Catalog & Discovery

* Home banners
* Modules
* Module details
* Module stores
* Store details
* Store banners
* Store sections
* Section products
* Product details

### Shopping Cart

* Retrieve cart
* Add item
* Update item quantity
* Remove item
* Clear cart

### Favorites

* Add/remove favorite products
* List favorite products
* Add/remove favorite stores
* List favorite stores

### Checkout & Payments

* Create Stripe checkout sessions
* Process Stripe webhooks
* Handle payment status updates

### Multi-Tenant Commerce

The backend is designed around a shared infrastructure model where multiple stores/tenants participate in the same application while application and persistence concerns remain separated from tenant-specific business data.

---

## API Surface

### Authentication

```http
POST /api/auth/guest-session
POST /api/auth/register
POST /api/auth/verify-email
POST /api/auth/resend-verification
POST /api/auth/login
POST /api/auth/google
POST /api/auth/refresh
POST /api/auth/logout
POST /api/auth/forgot-password
POST /api/auth/reset-password
```

### Customer

```http
GET /api/customers/me
PUT /api/customers/me
PUT /api/customers/me/password
```

### Addresses

```http
GET    /api/addresses
GET    /api/addresses/{id}
POST   /api/addresses
PUT    /api/addresses/{id}
DELETE /api/addresses/{id}
POST   /api/addresses/{id}/set-default
```

### Catalog

```http
GET /api/home/banners

GET /api/modules
GET /api/modules/{id}
GET /api/modules/{id}/stores

GET /api/stores/{id}
GET /api/stores/{id}/banners
GET /api/stores/{id}/sections

GET /api/sections/{id}/products
GET /api/products/{id}
```

### Cart

```http
GET    /api/cart
POST   /api/cart/items
PUT    /api/cart/items/{id}
DELETE /api/cart/items/{id}
DELETE /api/cart
```

### Favorites

```http
POST   /api/favorites/products/{id}
DELETE /api/favorites/products/{id}
GET    /api/favorites/products

POST   /api/favorites/stores/{id}
DELETE /api/favorites/stores/{id}
GET    /api/favorites/stores
```

### Checkout

```http
POST /api/checkout
POST /api/webhooks/stripe
```

---

## Project Structure

```text
.
├── .github/
│   └── workflows/
│       └── tests.yml
│
├── src/
│   ├── Api/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Properties/
│   │   ├── Program.cs
│   │   ├── Api.csproj
│   │   └── appsettings.example.json
│   │
│   ├── Application/
│   │   ├── Common/
│   │   ├── Features/
│   │   ├── Dependencyinjection.cs
│   │   └── Application.csproj
│   │
│   ├── Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Interfaces/
│   │   └── Domain.csproj
│   │
│   ├── Infrastructure/
│   │   ├── Persistence/
│   │   ├── Queries/
│   │   ├── Services/
│   │   ├── Settings/
│   │   ├── Dependencyinjection.cs
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── Infrastructure.csproj
│   │
│   └── schema/
│       ├── api_reference.md
│       └── multitenant_ecommerce_schema_v6.sql
│
├── tests/
│   ├── Api.Tests/
│   ├── Application.Tests/
│   ├── Domain.Tests/
│   └── Infrastructure.Tests/
│
├── Dockerfile
├── docker-compose.yml
├── multi-tenant-stores-backend.slnx
└── README.md
```

### Layer responsibilities

| Layer              | Responsibility                                                                        |
| ------------------ | ------------------------------------------------------------------------------------- |
| **API**            | HTTP endpoints, controllers, middleware, authentication, CORS, rate limiting, OpenAPI |
| **Application**    | Commands, queries, handlers, DTOs, validation, application workflows                  |
| **Domain**         | Entities, enums, business concepts, domain contracts                                  |
| **Infrastructure** | PostgreSQL, EF Core, Redis, authentication, email, Stripe, Google integration         |
| **Schema**         | Database schema and API reference artifacts                                           |

---

## Technology Stack

| Technology                | Purpose                                    |
| ------------------------- | ------------------------------------------ |
| **.NET 10**               | Application runtime                        |
| **ASP.NET Core**          | Web API and hosting                        |
| **MediatR**               | CQRS request dispatching                   |
| **FluentValidation**      | Request validation                         |
| **Entity Framework Core** | ORM and persistence                        |
| **Npgsql**                | PostgreSQL provider                        |
| **PostgreSQL 18**         | Primary relational database                |
| **Redis 7**               | Caching and session/verification workflows |
| **JWT**                   | Authentication and authorization           |
| **MailKit**               | Email delivery                             |
| **Stripe .NET**           | Checkout and payment integration           |
| **Google.Apis.Auth**      | Google ID token validation                 |
| **Docker Compose**        | Local infrastructure                       |
| **MiniProfiler**          | Development-time profiling                 |
| **Scalar**                | Interactive API documentation              |

---

## Authentication & Security

Authentication is implemented using JWT-based access tokens with configurable:

* Issuer
* Audience
* Signing key
* Access-token lifetime

Additional authentication workflows include:

* Email verification
* Password recovery
* Refresh tokens
* Logout
* Google authentication
* Guest sessions

The API also includes middleware-level concerns such as authentication, CORS, and rate limiting.

### External identity

Google ID tokens are validated using `Google.Apis.Auth`.

### Email workflows

MailKit is used for:

* Email verification
* Verification resend
* Password recovery

### Payment security

Stripe is integrated through checkout sessions and webhook processing, with webhook verification configured through the Stripe webhook secret.

> Never commit credentials, signing keys, connection strings, API keys, or production secrets to the repository.

---

## Infrastructure

### PostgreSQL

PostgreSQL is the primary persistence layer.

EF Core provides ORM-based persistence while the repository also contains database schema artifacts under:

```text
src/schema/
```

### Redis

Redis supports:

* Caching
* Session-related workflows
* Verification-related state

### Docker Compose

The local environment can run the API together with PostgreSQL and Redis through Docker Compose.

---

## Testing

The repository contains dedicated test projects for multiple architectural layers:

```text
tests/
├── Api.Tests/
├── Application.Tests/
├── Domain.Tests/
└── Infrastructure.Tests/
```

The test suite covers concerns including:

* API/controller behavior
* Application handlers
* Authentication workflows
* Domain behavior
* Infrastructure/database mapping

The repository also includes a GitHub Actions workflow that restores, builds, and runs the automated test suite.

Run all tests locally with:

```bash
dotnet test
```

Run a specific test project with:

```bash
dotnet test tests/Domain.Tests/Domain.Tests.csproj
dotnet test tests/Application.Tests/Application.Tests.csproj
dotnet test tests/Infrastructure.Tests/Infrastructure.Tests.csproj
dotnet test tests/Api.Tests/Api.Tests.csproj
```

---

## Local Development

### Prerequisites

Install:

* [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
* [Docker](https://www.docker.com/)
* Docker Compose

Optional:

* PostgreSQL client
* Redis CLI

### 1. Clone

```bash
git clone https://github.com/RashedKlo/multi-tenant-stores-backend.git

cd multi-tenant-stores-backend
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Configure application settings

Copy the example configuration:

```bash
cp src/Api/appsettings.example.json \
   src/Api/appsettings.Development.json
```

Configure the required local values.

Do not commit real secrets.

### 4. Start infrastructure

```bash
docker compose up --build
```

The Docker Compose environment exposes the API on:

```text
http://localhost:8080
```

### 5. Run the API directly

Alternatively:

```bash
dotnet run --project src/Api/Api.csproj
```

---

## Configuration

The application uses configuration sections for infrastructure and external integrations.

| Configuration                          | Required | Purpose                     |
| -------------------------------------- | -------: | --------------------------- |
| `ConnectionStrings__DefaultConnection` |      Yes | PostgreSQL connection       |
| `ConnectionStrings__Redis`             |      Yes | Redis connection            |
| `Jwt__Issuer`                          |      Yes | JWT issuer                  |
| `Jwt__Audience`                        |      Yes | JWT audience                |
| `Jwt__SigningKey`                      |      Yes | JWT signing key             |
| `Jwt__AccessTokenMinutes`              |      Yes | Access-token lifetime       |
| `Smtp__Host`                           |      Yes | SMTP server                 |
| `Smtp__Port`                           |      Yes | SMTP port                   |
| `Smtp__Username`                       | Optional | SMTP username               |
| `Smtp__Password`                       | Optional | SMTP password               |
| `Smtp__FromEmail`                      |      Yes | Sender email                |
| `Smtp__FromName`                       |      Yes | Sender display name         |
| `Smtp__UseSsl`                         |      Yes | SMTP TLS/SSL setting        |
| `GoogleAuth__ClientId`                 |      Yes | Google client ID            |
| `Stripe__SecretKey`                    |      Yes | Stripe secret key           |
| `Stripe__WebhookSecret`                |      Yes | Stripe webhook verification |
| `Stripe__SuccessUrl`                   |      Yes | Successful checkout URL     |
| `Stripe__CancelUrl`                    |      Yes | Canceled checkout URL       |
| `Stripe__Currency`                     |      Yes | Default Stripe currency     |

---

## API Documentation

The API uses ASP.NET Core OpenAPI generation with Scalar.

In development, the application exposes:

* OpenAPI document through `MapOpenApi()`
* Interactive API reference through Scalar

Start the application and use the development API documentation endpoint exposed by the application.

---

## Database

The repository currently includes database schema artifacts under:

```text
src/schema/
├── api_reference.md
└── multitenant_ecommerce_schema_v6.sql
```

The current repository snapshot does not contain a checked-in EF Core `Migrations` directory.

If EF Core migrations are available in your branch, the database can be updated with:

```bash
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

---

## Current Scope

The current implementation focuses on the customer-facing commerce journey:

```text
Authentication
      │
      ▼
Customer Profile
      │
      ▼
Store / Product Discovery
      │
      ▼
Shopping Cart
      │
      ▼
Favorites
      │
      ▼
Checkout
      │
      ▼
Stripe Payment
      │
      ▼
Webhook / Payment Status
```

### Not yet exposed

Order-related domain concepts and statuses exist in the domain model and checkout flow, but a dedicated order-management API is not currently exposed through the controller layer.

This is an intentional boundary of the current implementation rather than a claim that the order domain does not exist.

---

## Engineering Practices

The project is organized around several principles:

### Separation of concerns

Controllers remain focused on HTTP concerns while application handlers coordinate use cases.

### Dependency inversion

Application and domain layers depend on abstractions rather than concrete infrastructure implementations.

### Feature-oriented application layer

Application functionality is organized around business capabilities rather than large generic service classes.

### Centralized validation

FluentValidation is integrated into the MediatR pipeline so validation remains consistent across application requests.

### Explicit infrastructure boundaries

External systems such as PostgreSQL, Redis, Stripe, Google authentication, and SMTP are kept behind infrastructure implementations.

### Containerized development

Docker Compose provides a reproducible local infrastructure environment.

### Automated verification

The repository maintains separate tests for API, application, domain, and infrastructure concerns and runs them through GitHub Actions.

---

## Roadmap

The next areas of development are:

* Expand order-management APIs
* Increase integration-test coverage
* Add end-to-end commerce workflows
* Strengthen tenant-management capabilities
* Expand production deployment configuration
* Improve observability and operational diagnostics

The goal is to evolve the current backend foundation toward a more complete production commerce platform without compromising its architectural boundaries.

---

## Repository Philosophy

This project favors **explicit architecture over accidental complexity**.

The intent is not to demonstrate every possible .NET pattern. Instead, the architecture is designed around a few principles:

> **Business rules belong to the domain.**
> **Use cases belong to the application layer.**
> **HTTP concerns belong to the API layer.**
> **Infrastructure details stay replaceable.**

The result is a backend that can evolve as the commerce domain grows without turning controllers, persistence, and external integrations into a single tightly coupled system.

---

## Contributing

Contributions are welcome.

When contributing:

1. Keep changes focused and reviewable.
2. Respect the existing layer boundaries.
3. Keep business rules in the appropriate domain/application layer.
4. Avoid coupling application logic directly to infrastructure implementations.
5. Follow the existing DDD and CQRS conventions.
6. Add or update tests when changing behavior.

---

## License

This project is licensed under the **MIT License**.

See [`LICENSE`](./LICENSE) for details.

---

## Author

**Rashed Klo**

GitHub: [@RashedKlo](https://github.com/RashedKlo)

---

## Project Highlights

**Architecture**

`DDD` · `CQRS` · `MediatR` · `Layered Architecture`

**Backend**

`.NET 10` · `ASP.NET Core`

**Persistence**

`PostgreSQL` · `EF Core` · `Npgsql`

**Infrastructure**

`Redis` · `Docker` · `Docker Compose`

**Security & Identity**

`JWT` · `Google Authentication` · `Email Verification` · `Password Recovery`

**Payments**

`Stripe Checkout` · `Stripe Webhooks`

**API Engineering**

`OpenAPI` · `Scalar` · `FluentValidation` · `Rate Limiting`

**Quality**

`Unit Tests` · `API Tests` · `Application Tests` · `Infrastructure Tests` · `GitHub Actions`

# Multi-Tenant Stores Backend

> **Scalable multi-tenant commerce backend built with .NET 10, ASP.NET Core, DDD, CQRS, MediatR, PostgreSQL, Redis, and Stripe.**






\

## Overview

**Multi-Tenant Stores Backend** is a scalable ASP.NET Core Web API designed for a multi-tenant commerce and marketplace platform.

The backend provides tenant-aware catalog and commerce capabilities including:

* Authentication and account management
* Customer profiles and addresses
* Store and product discovery
* Shopping carts
* Favorites
* Checkout
* Stripe payment integration
* Google authentication
* Email verification and password recovery
* Redis-backed caching and session-related workflows

The project is structured around **Domain-Driven Design (DDD)** and **CQRS**, with **MediatR** used to decouple HTTP endpoints from application commands and queries.

The goal is to provide a clean foundation for building commerce applications where multiple stores/tenants can operate within a shared backend infrastructure.

---

## Engineering Focus

This project focuses on several backend engineering principles:

* **Domain-Driven Design** — business concepts are represented through a dedicated domain layer.
* **CQRS** — commands and queries are separated to keep application use cases focused.
* **Clean separation of concerns** — API, application, domain, and infrastructure responsibilities are isolated.
* **Dependency Injection** — application and infrastructure services are registered through dedicated composition modules.
* **Validation pipelines** — FluentValidation is integrated into the MediatR pipeline.
* **External service integration** — Stripe, Google authentication, SMTP, Redis, and PostgreSQL are isolated behind infrastructure services.
* **Containerized development** — Docker Compose provides a reproducible local environment.
* **API-first development** — OpenAPI and Scalar provide an interactive API documentation experience.

---

## Architecture

The application follows a layered architecture:

```text
                         ┌─────────────────────┐
                         │      Client(s)       │
                         │ Web / Mobile / etc.  │
                         └──────────┬──────────┘
                                    │
                                    ▼
                         ┌─────────────────────┐
                         │   ASP.NET Core API  │
                         │ Controllers / HTTP  │
                         └──────────┬──────────┘
                                    │
                                    ▼
                         ┌─────────────────────┐
                         │       MediatR       │
                         │   Commands/Queries  │
                         └──────────┬──────────┘
                                    │
                     ┌──────────────┴──────────────┐
                     │                             │
                     ▼                             ▼
              ┌──────────────┐             ┌──────────────┐
              │ Application  │             │    Domain    │
              │ Use Cases    │────────────▶│   Business   │
              │ DTOs         │             │    Model     │
              │ Validation   │             │   Contracts  │
              └──────┬───────┘             └──────────────┘
                     │
                     ▼
              ┌──────────────┐
              │Infrastructure│
              │              │
              │ EF Core      │
              │ PostgreSQL   │
              │ Redis        │
              │ JWT          │
              │ Stripe       │
              │ SMTP         │
              │ Google Auth  │
              └──────────────┘
```

### Request Flow

A typical request follows this path:

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
     ├───────────────┐
     ▼               ▼
Domain Logic     Infrastructure
                     │
          ┌──────────┼──────────┐
          ▼          ▼          ▼
      PostgreSQL   Redis     External APIs
```

This structure keeps HTTP concerns separate from business logic and infrastructure implementation details.

---

## Project Structure

```text
.
├── Dockerfile
├── docker-compose.yml
├── multi-tenant-stores-backend.slnx
├── README.md
│
├── src/
│   │
│   ├── Api/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Properties/
│   │   ├── Program.cs
│   │   ├── Api.csproj
│   │   ├── appsettings.json
│   │   ├── appsettings.example.json
│   │   └── Api.http
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
└── tests/
    └── Not currently included
```

### Layer Responsibilities

| Layer              | Responsibility                                                                           |
| ------------------ | ---------------------------------------------------------------------------------------- |
| **API**            | HTTP endpoints, controllers, middleware, authentication, CORS, rate limiting and OpenAPI |
| **Application**    | Commands, queries, handlers, DTOs, validation and application workflows                  |
| **Domain**         | Entities, enums, business concepts and domain contracts                                  |
| **Infrastructure** | Database access, Redis, authentication services, email, Stripe and Google integrations   |
| **Schema**         | Database reference documentation and SQL schema artifacts                                |

---

## Core Features

### Authentication

* [x] Guest sessions
* [x] User registration
* [x] Email verification
* [x] Verification email resend
* [x] Login
* [x] Google authentication
* [x] Access token refresh
* [x] Logout
* [x] Forgot password
* [x] Password reset

### Customer Management

* [x] Retrieve current customer profile
* [x] Update customer profile
* [x] Change password

### Address Management

* [x] List addresses
* [x] Retrieve address
* [x] Create address
* [x] Update address
* [x] Delete address
* [x] Set default address

### Catalog & Discovery

* [x] Home banners
* [x] Modules
* [x] Module details
* [x] Module stores
* [x] Store details
* [x] Store banners
* [x] Store sections
* [x] Section products
* [x] Product details

### Shopping Cart

* [x] Retrieve cart
* [x] Add cart item
* [x] Update cart item
* [x] Remove cart item
* [x] Clear cart

### Favorites

* [x] Add/remove favorite products
* [x] List favorite products
* [x] Add/remove favorite stores
* [x] List favorite stores

### Checkout & Payments

* [x] Checkout session creation
* [x] Stripe webhook processing
* [x] Payment status handling

### Planned / Not Yet Exposed

* [x] Order management API

> Order-related entities and statuses exist in the domain model and checkout flow, but order management endpoints are not currently exposed through the controller layer.

---

## API Endpoints

### Authentication

```text
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

```text
GET /api/customers/me
PUT /api/customers/me
PUT /api/customers/me/password
```

### Addresses

```text
GET    /api/addresses
GET    /api/addresses/{id}
POST   /api/addresses
PUT    /api/addresses/{id}
DELETE /api/addresses/{id}
POST   /api/addresses/{id}/set-default
```

### Catalog

```text
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

```text
GET    /api/cart
POST   /api/cart/items
PUT    /api/cart/items/{id}
DELETE /api/cart/items/{id}
DELETE /api/cart
```

### Favorites

```text
POST   /api/favorites/products/{id}
DELETE /api/favorites/products/{id}
GET    /api/favorites/products

POST   /api/favorites/stores/{id}
DELETE /api/favorites/stores/{id}
GET    /api/favorites/stores
```

### Checkout

```text
POST /api/checkout
POST /api/webhooks/stripe
```

---

## Technology Stack

| Technology                            | Role                                             |
| ------------------------------------- | ------------------------------------------------ |
| **.NET 10**                           | Target framework                                 |
| **ASP.NET Core Web API**              | HTTP API and application hosting                 |
| **MediatR**                           | CQRS command/query dispatching                   |
| **FluentValidation**                  | Application request validation                   |
| **Entity Framework Core**             | ORM and persistence                              |
| **Npgsql**                            | PostgreSQL provider                              |
| **PostgreSQL 18**                     | Primary relational database                      |
| **Redis 7**                           | Caching and session/verification-related storage |
| **JWT / ASP.NET Core Authentication** | Authentication and authorization                 |
| **MailKit**                           | Email delivery                                   |
| **Stripe .NET**                       | Checkout and webhook integration                 |
| **Google.Apis.Auth**                  | Google ID token validation                       |
| **Docker / Docker Compose**           | Local infrastructure orchestration               |
| **MiniProfiler**                      | Development-time request profiling               |
| **Scalar.AspNetCore**                 | Interactive API documentation                    |

---

## Authentication & External Integrations

The backend integrates with several external infrastructure components:

### JWT Authentication

JWT-based authentication is used for access-token validation and authorization.

Configuration includes:

* Issuer
* Audience
* Signing key
* Access-token lifetime

### Google Authentication

Google ID tokens are validated through `Google.Apis.Auth` to support Google-based authentication.

### Email

`MailKit` is used for authentication-related email workflows including:

* Email verification
* Verification resend
* Password recovery

### Redis

Redis is used for caching and verification/session-related workflows.

### Stripe

Stripe is integrated into the checkout flow for:

* Checkout session creation
* Webhook processing
* Payment-related status handling

---

## Local Development

### Prerequisites

Install the following:

* [.NET SDK 10.0](https://dotnet.microsoft.com/)
* [Docker](https://www.docker.com/)
* Docker Compose support

Optional:

* PostgreSQL client
* Redis CLI

---

## Getting Started

### 1. Clone the repository

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

Configure the required local values and **never commit real secrets**.

### 4. Start infrastructure

Start the API, PostgreSQL and Redis services:

```bash
docker compose up --build
```

### 5. Apply database changes

The current repository snapshot does not contain a checked-in EF Core `Migrations` directory.

The repository instead contains the database schema artifacts under:

```text
src/schema/
```

If EF Core migrations are available in your branch, the database can be updated with:

```bash
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

### 6. Run the API

```bash
dotnet run --project src/Api/Api.csproj
```

The Docker Compose configuration exposes the API on port:

```text
8080
```

---

## API Documentation

The API uses ASP.NET Core OpenAPI generation together with Scalar.

During development:

* OpenAPI is registered through `AddOpenApi()`.
* The OpenAPI document is exposed through `MapOpenApi()`.
* Scalar is exposed through `MapScalarApiReference()`.
* Scalar is enabled when the application is running in the Development environment.

Once the API is running, use the local development URL to access the interactive API documentation.

---

## Configuration

The application expects the following configuration values:

| Configuration                          | Required | Purpose                         |
| -------------------------------------- | -------: | ------------------------------- |
| `ConnectionStrings__DefaultConnection` |      Yes | PostgreSQL connection           |
| `ConnectionStrings__Redis`             |      Yes | Redis connection                |
| `Jwt__Issuer`                          |      Yes | JWT issuer                      |
| `Jwt__Audience`                        |      Yes | JWT audience                    |
| `Jwt__SigningKey`                      |      Yes | JWT signing key                 |
| `Jwt__AccessTokenMinutes`              |      Yes | Access-token lifetime           |
| `Smtp__Host`                           |      Yes | SMTP server                     |
| `Smtp__Port`                           |      Yes | SMTP port                       |
| `Smtp__Username`                       | Optional | SMTP username                   |
| `Smtp__Password`                       | Optional | SMTP password                   |
| `Smtp__FromEmail`                      |      Yes | Sender email                    |
| `Smtp__FromName`                       |      Yes | Sender display name             |
| `Smtp__UseSsl`                         |      Yes | SMTP TLS/SSL setting            |
| `GoogleAuth__ClientId`                 |      Yes | Google authentication client ID |
| `Stripe__SecretKey`                    |      Yes | Stripe secret key               |
| `Stripe__WebhookSecret`                |      Yes | Stripe webhook verification     |
| `Stripe__SuccessUrl`                   |      Yes | Successful checkout redirect    |
| `Stripe__CancelUrl`                    |      Yes | Canceled checkout redirect      |
| `Stripe__Currency`                     |      Yes | Default Stripe currency         |

**Never commit secrets, private keys, passwords, or production connection strings.**

---

## Testing

A dedicated automated test project is **not currently included** in the repository snapshot.

Running:

```bash
dotnet test
```

currently does not provide a concrete application test suite.

### Planned testing strategy

```text
Unit Tests
    │
    ├── Domain behavior
    ├── Application handlers
    └── Validation
         
Integration Tests
    │
    ├── PostgreSQL
    ├── Redis
    └── API endpoints

End-to-End Tests
    │
    └── Authentication → Catalog → Cart → Checkout
```

---

## Development Practices

The project is structured to encourage:

* Small, focused application use cases
* Clear separation between domain and infrastructure
* Dependency inversion through interfaces
* Centralized validation
* Feature-oriented application organization
* Explicit external service integrations
* Containerized local development
* Reviewable, scoped changes

---

## Roadmap

The following areas are candidates for future development:

* [ ] Automated unit tests
* [ ] Integration test suite
* [ ] End-to-end API tests
* [ ] Order management endpoints
* [ ] CI/CD pipeline
* [ ] Production deployment configuration
* [ ] Expanded observability
* [ ] Additional tenant-management capabilities

---

## Project Status

This repository represents an actively structured backend foundation for a multi-tenant commerce platform.

The current implementation already covers the core customer, authentication, discovery, cart, favorites, and checkout workflows, while some areas—particularly automated testing and order management APIs—remain to be expanded.

---

## Contributing

Contributions are welcome.

When contributing:

1. Keep changes focused and reviewable.
2. Follow the existing layered architecture.
3. Keep business logic within the appropriate application/domain boundaries.
4. Avoid coupling application logic directly to infrastructure implementations.
5. Maintain consistency with the existing DDD and CQRS patterns.

---

## License

A `LICENSE` file is not currently included in the repository.

The project's license is therefore **not yet specified**.

---

## Author

**Rashed Klo**

GitHub: [@RashedKlo](https://github.com/RashedKlo)

---

## Why This Project?

This project demonstrates practical backend engineering around a realistic commerce domain, including:

**Architecture**

DDD · CQRS · MediatR · Layered Architecture

**Persistence**

PostgreSQL · Entity Framework Core · Npgsql

**Performance & Infrastructure**

Redis · Docker · Docker Compose

**Security & Identity**

JWT · Google Authentication · Email Verification

**Payments**

Stripe Checkout · Stripe Webhooks

**API Engineering**

ASP.NET Core · OpenAPI · Scalar · Validation · Rate Limiting

The emphasis is not only on exposing endpoints, but on organizing a backend that can evolve as the commerce domain and tenant requirements grow.

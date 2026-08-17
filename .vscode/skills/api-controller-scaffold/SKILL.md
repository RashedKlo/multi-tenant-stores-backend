---
name: api-controller-scaffold
description: Use when adding a new Web API controller for an Application feature in this Clean Architecture .NET project. Generates a standard CRUD controller matching the existing TenantsController style and route conventions.
---

# API Controller Scaffold

This skill creates a new ASP.NET Core controller for a feature that already has Application commands and queries. It must follow the exact existing controller pattern used by `TenantsController`.

## Controller structure

- Namespace: `Api.Controllers`
- Route: `[Route("api/{featurePluralLower}")]`
- Rate limiting: `[EnableRateLimiting("fixed")]`
- Constructor injection: `IMediator _mediator`
- Methods:
  - `GetAll` -> `GET /api/{featurePluralLower}` -> `GetAll{FeaturePlural}Query`
  - `GetById` -> `GET /api/{featurePluralLower}/{id:guid}` -> `Get{Feature}ByIdQuery`
  - `Create` -> `POST /api/{featurePluralLower}` -> `Create{Feature}Command`
  - `Update` -> `PUT /api/{featurePluralLower}/{id:guid}` -> `Update{Feature}Command`
  - `Delete` -> `DELETE /api/{featurePluralLower}/{id:guid}` -> `Delete{Feature}Command`

## Response behavior

- `GetAll` returns `Ok(result)`
- `GetById` returns `Ok(result)` or `NotFound(...)`
- `Create` returns `CreatedAtAction(nameof(GetById), new { id = result.Id }, result)`
- `Update` checks `id != command.Id` and returns `BadRequest(...)`; if result is null returns `NotFound(...)`, otherwise `Ok(result)`
- `Delete` returns `NoContent()` if deleted, otherwise `NotFound(...)`

## Imports

The controller should import:
- `using MediatR;`
- `using Microsoft.AspNetCore.Mvc;`
- `using Microsoft.AspNetCore.RateLimiting;`
- application feature namespaces for the command/query types.

## Notes

- Use `CreatedAtAction(nameof(GetById), new { id = result.Id }, result)` for create responses.
- Keep controller methods minimal: only mediator calls and HTTP response mapping.
- Use the same XML-style summaries as `TenantsController`.

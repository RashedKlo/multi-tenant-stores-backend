---
name: application-feature-scaffold
description: Use when adding a new feature, entity, command, query, or CRUD operation to this Clean Architecture + CQRS + MediatR .NET project (Domain, Infrastructure, and Application layers). Covers exact folder structure and conventions for Domain entities, repository interfaces/implementations, EF Core configurations, Commands, Queries, Handlers, Validators, shared DTOs, and DI registration, so new features match existing ones (Product, Category) exactly. Trigger on requests like "add a new feature for X", "add a create/update/delete command for X", "add a query to get X", "add entity X", or "implement CRUD for X".
---

# Full-Stack Feature Scaffold (Domain → Infrastructure → Application)

This skill reproduces the exact structure used for the `Products` and
`Categories` features. Follow it exactly — do not introduce a different
folder layout, naming scheme, delete strategy, or mapping style even if it
seems "cleaner." Consistency with existing features is the goal.

## Layer order for a new entity

Build in this order — each step depends on the previous one:

1. **Domain entity** (`Domain/Entities/{Entity}.cs`)
2. **Domain repository interface** (`Domain/Interfaces/I{Entity}Repository.cs`)
3. **Infrastructure repository implementation** (`Infrastructure/Persistence/{Entity}Repository.cs`)
4. **Infrastructure EF configuration** (`Infrastructure/Persistence/Configurations/{Entity}Configuration.cs`)
5. **Application DTO** (`Application/Common/DTOs/{Entity}Dto.cs`)
6. **Application Commands/Queries + Handlers + Validators**
7. **DI registration** (`Application/DependencyInjection.cs`)

## Step 1 — Domain entity

Two shapes exist in this codebase — pick based on whether the table needs
audit timestamps and soft-delete:

**Simple entity (no timestamps, hard-delete)** — e.g. `Category`:
```csharp
namespace Domain.Entities;

public class {Entity}
{
    public Guid Id { get; private set; }
    public Guid {ParentFk} { get; private set; }
    public string NameEn { get; private set; } = string.Empty;
    public string NameAr { get; private set; } = string.Empty;
    // ...other fields
    public bool IsActive { get; private set; }

    private {Entity}() { }

    public static {Entity} Create(Guid {parentFk}, string nameEn, string nameAr, /* ... */ bool isActive)
    {
        return new {Entity}
        {
            Id = Guid.NewGuid(),
            {ParentFk} = {parentFk},
            NameEn = nameEn,
            NameAr = nameAr,
            IsActive = isActive
        };
    }

    public void Update(Guid {parentFk}, string nameEn, string nameAr, /* ... */ bool isActive)
    {
        {ParentFk} = {parentFk};
        NameEn = nameEn;
        NameAr = nameAr;
        IsActive = isActive;
    }
}
```

**Full entity (audit timestamps + soft-delete)** — e.g. `Product`: same shape
plus `CreatedAt`, `UpdatedAt`, `DeletedAt` properties and a `SoftDelete()`
method that sets `DeletedAt = DateTime.UtcNow` (and typically `IsActive =
false`). Use this shape only if the database table actually has those
columns — check the schema, don't add them by default.

Rules:
- All properties `{ get; private set; }` — **never** a public setter.
- Only two ways to mutate: the static `Create` factory and the instance
  `Update` method. No other public mutation method except `SoftDelete()` on
  entities that support it.
- `Update` takes the **full field set** (not a partial/patch) — same as
  `Create` minus `Id`.

## Step 2 — Domain repository interface

```csharp
using Domain.Entities;

namespace Domain.Interfaces;

public interface I{Entity}Repository
{
    Task<List<{Entity}>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<{Entity}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    // + any natural scoped-query method, e.g.:
    Task<List<{Entity}>> GetBy{ParentEntity}IdAsync(Guid {parentFk}, CancellationToken cancellationToken = default);
    Task AddAsync({Entity} entity, CancellationToken cancellationToken = default);
    void Update({Entity} entity);
    void Delete({Entity} entity);              // <- only include this if the entity hard-deletes (no SoftDelete())
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```
- Base 6 methods always present: `GetAllAsync`, `GetByIdAsync`, `AddAsync`,
  `Update` (sync `void`), `SaveChangesAsync`. Add `Delete` (sync `void`) only
  for hard-delete entities.
- Add one scoped query method per natural parent relationship (e.g.
  `GetByModuleIdAsync` for `Category`, `GetBySectionIdAsync` for `Product`) —
  used both by the handler and to build the right cache key.

## Step 3 — Infrastructure repository implementation

```csharp
using Domain.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class {Entity}Repository : I{Entity}Repository
{
    private readonly AppDbContext _context;

    public {Entity}Repository(AppDbContext context) => _context = context;

    public async Task<List<{Entity}>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Set<{Entity}>().ToListAsync(cancellationToken);

    public async Task<{Entity}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Set<{Entity}>().FindAsync(new object?[] { id }, cancellationToken: cancellationToken);

    public async Task<List<{Entity}>> GetBy{ParentEntity}IdAsync(Guid {parentFk}, CancellationToken cancellationToken = default)
        => await _context.Set<{Entity}>().Where(e => e.{ParentFk} == {parentFk}).ToListAsync(cancellationToken);

    public async Task AddAsync({Entity} entity, CancellationToken cancellationToken = default)
        => await _context.Set<{Entity}>().AddAsync(entity, cancellationToken);

    public void Update({Entity} entity) => _context.Set<{Entity}>().Update(entity);
    public void Delete({Entity} entity) => _context.Set<{Entity}>().Remove(entity);   // hard-delete entities only

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
```
- Always use `_context.Set<{Entity}>()`, never a named `DbSet` property —
  matches the existing convention exactly.
- One-line expression bodies (`=>`) for every method here — no method has a
  `{ }` block body in this class, keep that style.

## Step 4 — EF Core configuration

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>
{
    public void Configure(EntityTypeBuilder<{Entity}> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.{ParentFk}).IsRequired();

        builder.Property(e => e.NameEn).IsRequired().HasMaxLength(150);
        builder.Property(e => e.NameAr).IsRequired().HasMaxLength(150);

        // optional string fields: HasMaxLength only, no IsRequired
        builder.Property(e => e.ImageUrl).HasMaxLength(500);

        // fields with a DB default: mirror it here
        builder.Property(e => e.DisplayOrder).HasDefaultValue(0);
        builder.Property(e => e.IsActive).HasDefaultValue(true);

        builder.HasIndex(e => e.{ParentFk});
        builder.HasIndex(e => e.NameEn);
        builder.HasIndex(e => e.NameAr);

        // mirrors a UNIQUE(parent_fk, name_en/name_ar) constraint from the SQL schema
        builder.HasAlternateKey(e => new { e.{ParentFk}, e.NameEn });
        builder.HasAlternateKey(e => new { e.{ParentFk}, e.NameAr });
    }
}
```
- Every `HasMaxLength(N)` must match the column length in the SQL schema
  exactly, and must match the Validator's `MaximumLength(N)` — all three
  (schema, EF config, validator) stay in lock-step. If the schema has a
  `UNIQUE(parent, name_en)` / `UNIQUE(parent, name_ar)` pair, mirror both as
  separate `HasAlternateKey` calls (not one composite key across both
  languages).

## Step 5 — DTO (shared, `Application/{Entity}/DTOs/`)

Same as before: one `{Entity}Dto` record per entity, every persisted field,
reused by every Command/Query for that entity. Never a per-operation DTO
variant.

## Step 6 — Commands / Queries / Handlers / Validators

Same folder structure and per-file conventions as before:
```
Application/{Entities}/Commands/Create{Entity}/   (Command + Handler + Validator)
Application/{Entities}/Commands/Update{Entity}/   (Command + Handler + Validator)
Application/{Entities}/Commands/Delete{Entity}/   (Command + Handler — no validator)
Application/{Entities}/Queries/GetAll{Entities}/  (Query + Handler)
Application/{Entities}/Queries/Get{Entity}ById/   (Query + Handler)
```

**Delete handler — pick the strategy that matches Step 1's entity shape:**
- Hard-delete entity (e.g. Category): `_repository.Delete(entity)` then
  `SaveChangesAsync`.
- Soft-delete entity (e.g. Product): `entity.SoftDelete()`, then
  `_repository.Update(entity)`, then `SaveChangesAsync`.

Everything else (cache-key naming, manual DTO mapping, cache-first on list
queries only, no `Result<T>` wrapper, classic constructor injection) is
unchanged from the original Products-based conventions.

## Step 7 — DI registration (`Application/DependencyInjection.cs`)

This is the step most likely to be forgotten — do it every time:

- **MediatR: do nothing.** `RegisterServicesFromAssembly` auto-discovers every
  `IRequestHandler` in the assembly. Do not add per-handler registration.
- **FluentValidation: add two explicit lines**, one per new Command
  Validator, following the existing list (each entity gets a
  `Create{Entity}Validator` line and an `Update{Entity}Validator` line):
  ```csharp
  services.AddValidatorsFromAssemblyContaining<Create{Entity}Validator>();
  services.AddValidatorsFromAssemblyContaining<Update{Entity}Validator>();
  ```
  Yes, technically one call registers the whole assembly — add the explicit
  per-validator lines anyway, matching the existing style exactly. Do not
  "clean this up" to a single call; that would diverge from the established
  pattern even though it's redundant.

## Full step-by-step for a brand-new entity

1. Check the SQL schema for the entity's columns, defaults, and constraints
   — this drives every other layer.
2. Domain entity (Step 1) — simple or full shape based on whether the schema
   has `created_at`/`updated_at`/`deleted_at`.
3. Domain repository interface (Step 2).
4. Infrastructure repository implementation (Step 3).
5. EF configuration (Step 4) — lengths/defaults/indexes/alternate keys
   matching the schema exactly.
6. DTO (Step 5).
7. Commands/Queries/Handlers/Validators (Step 6) — only the operations
   actually requested.
8. DI registration (Step 7) — add the two FluentValidation lines; nothing
   needed for MediatR.
9. If anything about fields, parent-scope naming, or delete strategy is
   ambiguous, ask — don't guess and diverge from the pattern.

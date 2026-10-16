# Data Layer Architecture

All persistence code lives under `VerticalSliceFastEndpoints/Data/`. Feature slices never own database schema; they consume `ApplicationDbContext`.

## Folder layout

```
Data/
├── ApplicationDbContext.cs          # DbContext (Identity + DbSets), partial
├── Configurations/                  # IEntityTypeConfiguration<T> per entity
├── Custom/                          # Hand-written partial-class extensions
│   ├── ApplicationDbContext.Custom.cs   # SaveChanges* overrides (audit + soft delete)
│   └── Product.Custom.cs                # Navigation properties for Product
├── Initialization/                  # Startup migrate/seed pipeline
└── Migrations/                      # EF Core migration history (generated)
```

Entities themselves live in `Entities/`; shared contracts (`IAuditableEntity`, `ISoftDeletable`, `ICurrentUserService`) live in `Core/Interfaces/`.

## Migrations

- Location: `Data/Migrations/`, namespace `VerticalSliceFastEndpoints.Data.Migrations`.
- Never edit generated `*.Designer.cs` files or `ApplicationDbContextModelSnapshot.cs` by hand. Custom SQL goes in the `Up`/`Down` of the migration `.cs` file only.
- EF discovers migrations by assembly, so the folder move needs no configuration. New migrations are scaffolded next to the existing snapshot; pass `--output-dir` to be explicit:

```bash
dotnet ef migrations add <Name> --project VerticalSliceFastEndpoints --output-dir Data/Migrations
dotnet ef database update --project VerticalSliceFastEndpoints
```

## Partial-class customization (`Data/Custom/`)

Generated or scaffold-like types are split so hand-written behavior is kept apart:

- `ApplicationDbContext.Custom.cs` overrides `SaveChanges`/`SaveChangesAsync` to:
  - stamp `IAuditableEntity` (`Created*`/`Updated*`) with the id from `ICurrentUserService`, falling back to `"System"`;
  - convert deletes of `ISoftDeletable` entities into soft-delete updates.
- `Product.Custom.cs` adds navigation properties to the `Product` entity.

Rules: don't hard-delete soft-deletable entities, and don't set audit fields manually.

## Entity configuration (`Data/Configurations/`)

One `IEntityTypeConfiguration<T>` class per entity (e.g. `ProductConfiguration`). Keep mapping (keys, lengths, indexes, relationships) here rather than in data annotations or `OnModelCreating`.

## Initialization (`Data/Initialization/`)

`DatabaseInitializerExtensions` runs `ApplicationDbContextInitializer` at startup, driven by the `DatabaseInitializer` config section:

| Option | Effect |
|---|---|
| `Migrate` | Apply pending migrations |
| `Seed` | Seed initial data |
| `ExitAfterInitialization` | Exit before serving (migration-job mode) |

Enabled in `appsettings.Development.json`. Keep `Migrate` off in production and run migrations as a separate job.

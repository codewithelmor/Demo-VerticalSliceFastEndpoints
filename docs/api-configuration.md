# API Configuration (`Program.cs`)

Cross-cutting HTTP behavior is wired once in `Program.cs`; slices only opt in. See [features.md](features.md) for per-endpoint settings.

## Routing and versioning

- Routes are `/api/v{version}/...`: `RoutePrefix = "api"`, `Versioning.Prefix = "v"`, `PrependToRoute = true`. Endpoints declare only their own path (`Post("/products")`) plus `Version(n)`.
- Two OpenAPI documents (`v1`, `v2`) are split by `MinEndpointVersion`/`MaxEndpointVersion`.
- **New API version checklist:** add an `OpenApiDocument` for it, add a matching Scalar `AddDocument`, then use `Version(n)` in the slices.
- Scalar UI is the API reference at the app root / OpenAPI route.

## Content negotiation (JSON + XML)

Hand-rolled through `Serializer.RequestDeserializer` / `Serializer.ResponseSerializer`.

- DTOs must be XmlSerializer-friendly: public parameterless constructor, public settable properties.
- Use `[XmlRoot]`/`[XmlElement]` only when a schema requires it (see README).
- The global pre-processor `PreProcessors/AcceptHeaderValidator` returns `406 Not Acceptable` when `Accept` is set and contains none of `application/json`, `application/xml`, `text/xml` or `*/*`.

## Pre-processors

| Pre-processor | Scope | Purpose |
|---|---|---|
| `AcceptHeaderValidator` | Global (`IGlobalPreProcessor`) | 406 for unsupported `Accept` |
| `PreProcessorLogging<TReq>` | Per endpoint (`PreProcessor<...>()` in `Configure`) | Logs the request path |

New shared pre-processors go in `PreProcessors/`.

## CORS

Configured from the `Cors` section (a list of `CorsCompanySettings`) in `appsettings*.json` (`Company`, `AllowedOrigins`, `AllowedMethods`, `AllowedHeaders`):

- `AllowAll`: any origin, method and header.
- One policy per entry, named after `Company`.
- `RestrictedPolicy`: union of everything allowed by any company.

To onboard a client, add an entry to the config list; no code change is needed.

## Configuration and options

Strongly typed options live in `Options/` (`JwtSettings`, `CorsCompanySettings`, `DatabaseInitializerOptions`), each exposing a `SectionName` constant and bound with `builder.Services.Configure<T>(...)`. Required settings:

- `ConnectionStrings:DefaultConnection` (SQL Server)
- `ApiSettings` (`JwtSettings`): `Secret`, `Issuer`, `Audience`
- `DatabaseInitializer`: see [data-layer.md](data-layer.md)

## Dependency injection

Services are registered in `Program.cs` against interfaces from `Core/Interfaces/`, with implementations in `Infrastructure/` (`ICurrentUserService` → `CurrentUserService`, `IJwtTokenService` → `JwtTokenService`), all `AddScoped`.

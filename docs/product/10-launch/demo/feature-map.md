# Feature map: VerticalSliceFastEndpoints (repo commit 9a638cc)

Explored by running the app locally (`dotnet run`, LocalDB `FastEndpointsDemo`) and calling every endpoint, plus reading the source and docs.

| Area | Feature | Works? | Evidence | In video |
|---|---|---|---|---|
| Auth | POST /api/v1/auth/register (role Admin or Customer) | Yes | 200; duplicate returns 400 "Username already exists" | J3 |
| Auth | POST /api/v1/auth/login returns JWT (role claim, issuer, audience, 7-day expiry) | Yes | token decoded: role Admin, exp - nbf = 7 days | J3 |
| Validation | FluentValidation per slice (email, 12+ char password rules, title length) | Yes | 400 with per-rule messages | J3 |
| Products | POST /api/v1/products (Admin, throttle 20/min) | Yes | 401 without token, 201 with | J3 |
| Products | GET /api/v1/products (anonymous, paged) | Yes | JSON and XML | J3 |
| Products | DELETE /api/v1/products/{id} (soft delete) | Yes | 204; row stays with IsDeleted=1, DeletedBy stamped | J3 |
| Products | GET/PUT /products/{id}, POST /products/{id} (image upload) | Present, not exercised | | no |
| Users | GET /api/v1/Users (Dapper raw SQL) | Yes | 200 with roles | J3 |
| Content negotiation | JSON default, XML via Accept, 406 for unsupported | Yes | text/csv returns 406 | J3 |
| Versioning | v1 and v2 OpenAPI documents | v1 yes; v2 empty | /openapi/v2.json has `"paths": {}` | J3 (shown honestly) |
| Data layer | Audit stamps, soft delete, query filter | Yes | CreatedBy/DeletedBy resolve to the demo admin | J1, J3 |
| Startup | DatabaseInitializer Migrate + Seed (5 migrations, 2 roles) | Yes | log lines captured | J2 |
| Docs | Scalar at /scalar | Yes | | J3 |
| AI tooling | CLAUDE.md, /docs (4 files), .claude agents, commands, skills, Stop hook | Present | files read | J4 |

## Problems found in the app (for the tracker)

1. Docs mismatch: CLAUDE.md says the JWT section is `JwtSettings`; the code and appsettings use `ApiSettings` (`JwtSettings.SectionName = "ApiSettings"`). CLAUDE.md also says Scalar is "at the app root"; it is at `/scalar`.
2. NuGet audit: `System.Security.Cryptography.Pkcs 9.0.11` (transitive) has known vulnerabilities (NU1902 moderate, NU1903 high).
3. `GetUsersEndpoint` returns `totalCount = items.Count()` (page size), not the window-function total it computes.
4. `DeleteProductEndpoint` comment says uncommenting a line gives a hard delete; the DbContext converts deletes to soft deletes regardless.
5. `RegisterEndpoint` does not `return` after sending the duplicate/failed-create responses (works in practice, risky).
6. `CreateProductEndpoint.MapToEntity` has `createdByUserId` commented out; the audit stamp fills it, which works.
7. `GlobalUsings.cs` imports `VerticalSliceFastEndpoints.Features.User.GetUsers` while the folder is `Users`.
8. Login token lifetime is hard-coded to 7 days; `JwtSettings.ExpiryMinutes` (60) is not used by the login endpoint.
9. No automated tests (stated in CLAUDE.md).
10. README mentions `ApiSettings:Secret` but lists an empty `ImageUpload` folder in its tree; the tree script reports it out of date once other files are added (the Stop hook regenerates it).

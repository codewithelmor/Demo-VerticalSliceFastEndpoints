# Authentication and Authorization

ASP.NET Core Identity manages users; JWT bearer tokens secure the API.

## Components

| Piece | Location |
|---|---|
| `ApplicationUser` | `Entities/ApplicationUser.cs` (extends `IdentityUser`) |
| Roles | `Core/Constants/ApplicationRole.cs` |
| Claim names | `Core/Constants/JwtClaimTypes.cs` |
| Token service | `Core/Interfaces/IJwtTokenService.cs` → `Infrastructure/JwtTokenService.cs` |
| Current user | `ICurrentUserService` → `Infrastructure/CurrentUserService.cs` |
| Settings | `Options/JwtSettings.cs` (`Secret`, `Issuer`, `Audience`) |
| Endpoints | `Features/Auth/Register`, `Features/Auth/Login` |

JWT bearer validation and Identity registration are in `Program.cs`; the Identity tables are created by the migrations in `Data/Migrations/`.

## Flow

1. `POST /api/v1/auth/register` creates the user through `UserManager`.
2. `POST /api/v1/auth/login` checks the password with `SignInManager.CheckPasswordSignInAsync`, loads the user's role (fallback `Customer`) and returns a signed JWT (HMAC SHA-256) with `NameIdentifier`, `Role`, audience and issuer claims.
3. Clients send `Authorization: Bearer <token>`.

Both auth endpoints use `AllowAnonymous()`. Login failures return one generic message ("Username or password is incorrect") so the response doesn't reveal whether the user exists.

## Protecting endpoints

- Restrict by role with constants: `Roles(ApplicationRole.Admin)`. Never hard-code role strings.
- Public endpoints must say `AllowAnonymous()` explicitly.
- Pair protected write endpoints with `Throttle(...)`.
- In a handler, get the caller's id with `User.FindFirstValue(ClaimTypes.NameIdentifier)`; in services use `ICurrentUserService` (also used for audit stamps, falling back to `"System"`).

## Secrets

Outside Development, `ApiSettings:Secret` should come from user secrets or environment configuration. Don't commit real values to `appsettings*.json`.

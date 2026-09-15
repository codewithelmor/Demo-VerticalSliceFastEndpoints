# VerticalSliceFastEndpoints

A .NET 10 Web API built with [FastEndpoints](https://fast-endpoints.com/) using a **Vertical Slice Architecture**. Instead of grouping code by technical layer (Controllers, Services, Repositories), each feature is self-contained in its own folder with everything it needs — endpoint, request, response, and validator.

The API includes JWT-based authentication and role-based authorization (via ASP.NET Core Identity), EF Core with SQL Server, request/response versioning, and interactive API docs powered by Scalar + OpenAPI.

## Tech Stack

- **.NET 10** / ASP.NET Core Web API
- **[FastEndpoints](https://fast-endpoints.com/)** `8.3.0` — REPR (Request-Endpoint-Response) pattern instead of MVC controllers
- **FastEndpoints.OpenApi** — OpenAPI document generation
- **FluentValidation** `12.1.1` — request validation
- **Microsoft.AspNetCore.Identity + EntityFrameworkCore** — user management
- **JWT Bearer Authentication**
- **Entity Framework Core (SQL Server)** `10.0.11`
- **Dapper** `2.1.86` — for lightweight/raw queries where needed
- **[Scalar.AspNetCore](https://github.com/scalar/scalar)** — modern interactive API reference UI (replaces Swagger UI)

## Project Structure

```text
VerticalSliceFastEndpoints/
├── VerticalSliceFastEndpoints
│   ├── Data
│   │   ├── Configurations
│   │   │   └── ProductConfiguration.cs
│   │   └── ApplicationDbContext.cs
│   ├── Entities
│   │   ├── ApplicationUser.cs
│   │   └── Product.cs
│   ├── Features
│   │   ├── Auth
│   │   │   ├── Login
│   │   │   │   ├── LoginEndpoint.cs
│   │   │   │   ├── LoginRequest.cs
│   │   │   │   ├── LoginResponse.cs
│   │   │   │   └── LoginValidation.cs
│   │   │   └── Register
│   │   │       ├── RegisterEndpoint.cs
│   │   │       ├── RegisterRequest.cs
│   │   │       ├── RegisterResponse.cs
│   │   │       └── RegisterValidation.cs
│   │   ├── Product
│   │   │   ├── CreateProduct
│   │   │   │   ├── CreateProductEndpoint.cs
│   │   │   │   ├── CreateProductRequest.cs
│   │   │   │   ├── CreateProductResponse.cs
│   │   │   │   └── CreateProductValidation.cs
│   │   │   ├── DeleteProduct
│   │   │   │   └── DeleteProductEndpoint.cs
│   │   │   ├── GetProduct
│   │   │   │   ├── GetProductEndpoint.cs
│   │   │   │   └── GetProductResponse.cs
│   │   │   ├── GetProducts
│   │   │   │   ├── GetProductsEndpoint.cs
│   │   │   │   ├── GetProductsRequest.cs
│   │   │   │   ├── GetProductsResponse.cs
│   │   │   │   ├── GetProductsValidation.cs
│   │   │   │   └── ProductItemResponse.cs
│   │   │   ├── ImageUpload
│   │   │   ├── UpdateProduct
│   │   │   │   ├── UpdateProductEndpoint.cs
│   │   │   │   ├── UpdateProductRequest.cs
│   │   │   │   ├── UpdateProductResponse.cs
│   │   │   │   └── UpdateProductValidation.cs
│   │   │   └── UploadImage
│   │   │       ├── UploadImageEndpoint.cs
│   │   │       ├── UploadImageRequest.cs
│   │   │       ├── UploadImageResponse.cs
│   │   │       └── UploadImageValidation.cs
│   │   ├── Users
│   │   │   ├── GetUser
│   │   │   │   ├── GetUserEndpoint.cs
│   │   │   │   └── GetUserResponse.cs
│   │   │   └── GetUsers
│   │   │       ├── GetUsersEndpoint.cs
│   │   │       ├── GetUsersRequest.cs
│   │   │       ├── GetUsersResponse.cs
│   │   │       ├── GetUsersValidation.cs
│   │   │       └── UserItemResponse.cs
│   │   └── TestEndpoint.cs
│   ├── Migrations
│   │   ├── 20260903092129_InitialCreate.cs
│   │   ├── 20260903092129_InitialCreate.Designer.cs
│   │   ├── 20260903103558_AddedIdentitySchema.cs
│   │   ├── 20260903103558_AddedIdentitySchema.Designer.cs
│   │   ├── 20260904102330_LinkProductToUser.cs
│   │   ├── 20260904102330_LinkProductToUser.Designer.cs
│   │   ├── 20260904110845_ImageUploadToProduct.cs
│   │   ├── 20260904110845_ImageUploadToProduct.Designer.cs
│   │   └── ApplicationDbContextModelSnapshot.cs
│   ├── PreProcessors
│   │   └── PreProcessorLogging.cs
│   ├── Properties
│   │   └── launchSettings.json
│   ├── appsettings.Development.json
│   ├── appsettings.json
│   ├── GlobalUsings.cs
│   ├── Program.cs
│   ├── SD.cs
│   └── VerticalSliceFastEndpoints.csproj
├── .gitignore
├── global.json
└── VerticalSliceFastEndpoints.slnx
```

### Folder guide

| Folder | Purpose |
|---|---|
| `Data/` | `ApplicationDbContext` and EF Core entity configurations |
| `Entities/` | Domain/EF Core entities (`ApplicationUser`, `Product`) |
| `Features/` | One folder per feature slice, grouped by aggregate (`Auth`, `Product`, `Users`) — each slice owns its endpoint, request, response, and validator |
| `Migrations/` | EF Core migration history |
| `PreProcessors/` | FastEndpoints pre-processors (e.g. request logging) shared across endpoints |
| `SD.cs` | Static/shared definitions, e.g. role name constants (`SD.Role_Admin`) |

## Feature Slice Anatomy

Every feature slice under `Features/` follows the same four-file pattern. Using `Product/CreateProduct` as the reference example:

- **`CreateProductRequest.cs`** — the input DTO bound from the HTTP request body.
- **`CreateProductValidation.cs`** — a `FluentValidation` `Validator<TRequest>` enforcing field rules (required, length, etc.) before the handler runs.
- **`CreateProductResponse.cs`** — the output DTO returned to the client.
- **`CreateProductEndpoint.cs`** — the FastEndpoints `Endpoint` (here, an `EndpointWithMapping<TRequest, TResponse, TEntity>`) that configures routing/auth/versioning and implements the handler logic, including request↔entity mapping.

### Example: `CreateProduct` slice

```csharp
// CreateProductRequest.cs
public class CreateProductRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// CreateProductValidation.cs
public class CreateProductValidation : Validator<CreateProductRequest>
{
    public CreateProductValidation()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(1).WithMessage("Title must be at least 1 character long.")
            .MaximumLength(50).WithMessage("Title cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(100).WithMessage("Description cannot exceed 100 characters.");
    }
}

// CreateProductResponse.cs
public class CreateProductResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
}

// CreateProductEndpoint.cs
public class CreateProductEndpoint : EndpointWithMapping<CreateProductRequest, CreateProductResponse, Entity>
{
    private readonly ApplicationDbContext _context;

    public CreateProductEndpoint(ApplicationDbContext context) => _context = context;

    public override void Configure()
    {
        PreProcessor<PreProcessorLogging<CreateProductRequest>>();
        Post("/products");
        Version(1);
        Roles(SD.Role_Admin);
        Throttle(hitLimit: 20, durationSeconds: 60);

        Description(x => x
            .WithTags("Products")
            .WithDescription("Creates a new product in the system.")
            .Produces<CreateProductResponse>(201)
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var product = MapToEntity(req);
        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);

        var response = MapFromEntity(product);
        await Send.CreatedAtAsync("/products/{id}", new { id = response.Id }, response, cancellation: ct);
    }

    public override Entity MapToEntity(CreateProductRequest req)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return new Entity(req.Title, req.Description, createdByUserId: userId);
    }

    public override CreateProductResponse MapFromEntity(Entity entity) =>
        new() { Id = entity.Id, Title = entity.Title, Description = entity.Description };
}
```

Key things this slice demonstrates:
- **Role-based authorization** via `Roles(SD.Role_Admin)`.
- **API versioning** via `Version(1)`.
- **Rate limiting** via `Throttle(hitLimit, durationSeconds)`.
- **Pre-processors** for cross-cutting concerns like logging, applied per-endpoint.
- **OpenAPI metadata** (`Description(...)`) for tagging and documenting responses directly on the endpoint.
- **Explicit entity mapping** (`MapToEntity` / `MapFromEntity`) rather than a separate mapper class or AutoMapper.

## Application Startup (`Program.cs`)

`Program.cs` wires up the cross-cutting infrastructure shared by all feature slices:

- **Identity** — `AddIdentity<ApplicationUser, IdentityRole>()` backed by EF Core stores.
- **EF Core / SQL Server** — `ApplicationDbContext` registered against the `DefaultConnection` connection string.
- **CORS** — two named policies: a permissive `AllowAll` and a locked-down `RestrictedPolicy` scoped to specific origins/methods/headers.
- **JWT Bearer Authentication** — configured with a symmetric signing key from `ApiSettings:Secret`.
- **FastEndpoints** — registered with response caching, and configured with a route prefix (`/api`) and version prefix (`/v{n}`), so endpoints resolve to paths like `/api/v1/products`.
- **Dual OpenAPI documents** — separate `v1` and `v2` OpenAPI documents are registered, each filtered to only include endpoints within that version range (`MinEndpointVersion` / `MaxEndpointVersion`).
- **Scalar API Reference** — replaces Swagger UI, exposing both the v1 and v2 documents as selectable references in one UI.

```csharp
app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";     // -> /api
    c.Versioning.Prefix = "v";           // -> v
    c.Versioning.PrependToRoute = true;  // combined: /api/v1/...
});

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Product API v1")
        .AddDocument("v1", "Product API v1", "/openapi/v1.json")
        .AddDocument("v2", "Product API v2", "/openapi/v2.json");
});
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server (local or containerized)

### Setup

1. Clone the repository.
2. Update the connection string in `appsettings.Development.json` (`ConnectionStrings:DefaultConnection`).
3. Set your JWT signing secret in configuration (`ApiSettings:Secret`).
4. Apply EF Core migrations:
   ```bash
   dotnet ef database update
   ```
5. Run the API:
   ```bash
   dotnet run
   ```
6. Browse the API reference (Scalar UI) at the app's root/OpenAPI route, with both `v1` and `v2` documents available.

## Adding a New Feature Slice

To add a new feature, create a new folder under `Features/<Area>/<FeatureName>/` containing:

1. `<FeatureName>Request.cs` — input DTO
2. `<FeatureName>Response.cs` — output DTO
3. `<FeatureName>Validation.cs` — a `Validator<TRequest>` with FluentValidation rules
4. `<FeatureName>Endpoint.cs` — the FastEndpoints endpoint: configure route/version/auth/throttling in `Configure()`, implement logic in `HandleAsync()`

This keeps each feature isolated, easy to locate, and easy to delete/replace without touching unrelated code.

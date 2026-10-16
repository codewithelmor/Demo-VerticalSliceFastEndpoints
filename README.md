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
- **JSON + XML content negotiation** — custom `Serializer.RequestDeserializer` / `ResponseSerializer` hooks accept and return both `application/json` and `application/xml`

## Project Structure

```text
VerticalSliceFastEndpoints/
├── VerticalSliceFastEndpoints
│   ├── Core
│   │   ├── Constants
│   │   │   ├── ApplicationRole.cs
│   │   │   └── JwtClaimTypes.cs
│   │   └── Interfaces
│   │       ├── IAuditableEntity.cs
│   │       ├── ICurrentUserService.cs
│   │       ├── IImageUploadableEntity.cs
│   │       ├── IJwtTokenService.cs
│   │       └── ISoftDeletable.cs
│   ├── Data
│   │   ├── Configurations
│   │   │   └── ProductConfiguration .cs
│   │   ├── Custom
│   │   │   ├── ApplicationDbContext.Custom.cs
│   │   │   └── Product.Custom.cs
│   │   ├── Initialization
│   │   │   ├── ApplicationDbContextInitializer.cs
│   │   │   ├── DatabaseInitializerExtensions.cs
│   │   │   └── IDatabaseInitializer.cs
│   │   └── ApplicationDbContext.cs
│   ├── Entities
│   │   ├── ApplicationUser.cs
│   │   └── Product.cs
│   ├── Features
│   │   ├── Auth
│   │   │   ├── Login
│   │   │   │   ├── LoginEndpoint .cs
│   │   │   │   ├── LoginRequestDto.cs
│   │   │   │   ├── LoginResponseDto.cs
│   │   │   │   └── LoginValidation.cs
│   │   │   └── Register
│   │   │       ├── RegisterEndpoint.cs
│   │   │       ├── RegisterRequestDto.cs
│   │   │       ├── RegisterResponseDto.cs
│   │   │       └── RegisterValidation.cs
│   │   ├── Product
│   │   │   ├── CreateProduct
│   │   │   │   ├── CreateProductEndpoint.cs
│   │   │   │   ├── CreateProductRequestDto.cs
│   │   │   │   ├── CreateProductResponseDto.cs
│   │   │   │   └── CreateProductValidation.cs
│   │   │   ├── DeleteProduct
│   │   │   │   └── DeleteProductEndpoint.cs
│   │   │   ├── GetProduct
│   │   │   │   ├── GetProductEndpoint.cs
│   │   │   │   └── GetProductResponseDto.cs
│   │   │   ├── GetProducts
│   │   │   │   ├── GetProductsEndpoint.cs
│   │   │   │   ├── GetProductsRequestDto.cs
│   │   │   │   ├── GetProductsResponseDto.cs
│   │   │   │   ├── GetProductsValidation.cs
│   │   │   │   └── ProductItemResponseDto.cs
│   │   │   ├── ImageUpload
│   │   │   ├── UpdateProduct
│   │   │   │   ├── UpdateProductEndpoint.cs
│   │   │   │   ├── UpdateProductRequestDto.cs
│   │   │   │   ├── UpdateProductResponseDto.cs
│   │   │   │   └── UpdateProductValidation.cs
│   │   │   └── UploadImage
│   │   │       ├── UploadImageEndpoint.cs
│   │   │       ├── UploadImageRequestDto.cs
│   │   │       ├── UploadImageResponseDto.cs
│   │   │       └── UploadImageValidation.cs
│   │   ├── Users
│   │   │   ├── GetUser
│   │   │   │   ├── GetUserEndpoint.cs
│   │   │   │   └── GetUserResponseDto.cs
│   │   │   └── GetUsers
│   │   │       ├── GetUsersEndpoint.cs
│   │   │       ├── GetUsersRequestDto.cs
│   │   │       ├── GetUsersResponseDto.cs
│   │   │       ├── GetUsersValidation.cs
│   │   │       └── UserItemResponseDto.cs
│   │   └── TestEndpoint .cs
│   ├── Infrastructure
│   │   ├── CurrentUserService.cs
│   │   └── JwtTokenService .cs
│   ├── Migrations
│   │   ├── 20260903092129_InitialCreate.cs
│   │   ├── 20260903092129_InitialCreate.Designer.cs
│   │   ├── 20260903103558_AddedIdentitySchema.cs
│   │   ├── 20260903103558_AddedIdentitySchema.Designer.cs
│   │   ├── 20260904102330_LinkProductToUser.cs
│   │   ├── 20260904102330_LinkProductToUser.Designer.cs
│   │   ├── 20260904110845_ImageUploadToProduct.cs
│   │   ├── 20260904110845_ImageUploadToProduct.Designer.cs
│   │   ├── 20261002075515_UpdatedProductSchema1.cs
│   │   ├── 20261002075515_UpdatedProductSchema1.Designer.cs
│   │   └── ApplicationDbContextModelSnapshot.cs
│   ├── Options
│   │   ├── CorsCompanySettings.cs
│   │   ├── DatabaseInitializerOptions.cs
│   │   └── JwtSettings.cs
│   ├── PreProcessors
│   │   ├── AcceptHeaderValidator .cs
│   │   └── PreProcessorLogging.cs
│   ├── Properties
│   │   └── launchSettings.json
│   ├── appsettings.Development.json
│   ├── appsettings.json
│   ├── GlobalUsings.cs
│   ├── Program.cs
│   └── VerticalSliceFastEndpoints.csproj
├── .gitignore
├── global.json
├── README.md
└── VerticalSliceFastEndpoints.slnx
```

### Folder guide

| Folder | Purpose |
|---|---|
| `Data/` | `ApplicationDbContext` and EF Core entity configurations |
| `Entities/` | Domain/EF Core entities (`ApplicationUser`, `Product`) |
| `Features/` | One folder per feature slice, grouped by aggregate (`Auth`, `Product`, `Users`) — each slice owns its endpoint, request, response, and validator |
| `Migrations/` | EF Core migration history |
| `PreProcessors/` | FastEndpoints pre-processors shared across endpoints (request logging, `Accept` header validation) |
| `ApplicationRole.cs` | Static/shared definitions, e.g. role name constants (`ApplicationRole.Admin`) |

## Feature Slice Anatomy

Every feature slice under `Features/` follows the same four-file pattern. Using `Product/CreateProduct` as the reference example:

- **`CreateProductRequestDto.cs`** — the input DTO bound from the HTTP request body.
- **`CreateProductValidation.cs`** — a `FluentValidation` `Validator<TRequest>` enforcing field rules (required, length, etc.) before the handler runs.
- **`CreateProductResponseDto.cs`** — the output DTO returned to the client.
- **`CreateProductEndpoint.cs`** — the FastEndpoints `Endpoint` (here, an `EndpointWithMapping<TRequest, TResponse, TEntity>`) that configures routing/auth/versioning and implements the handler logic, including request↔entity mapping.

### Example: `CreateProduct` slice

```csharp
// CreateProductRequestDto.cs
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

// CreateProductResponseDto.cs
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
        Roles(ApplicationRole.Admin);
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
- **Role-based authorization** via `Roles(ApplicationRole.Admin)`.
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

## Content Negotiation (JSON + XML)

FastEndpoints only speaks JSON (`System.Text.Json`) out of the box — it has no built-in content negotiation like MVC's `AddXmlSerializerFormatters()`. This project adds `application/xml` support alongside JSON by overriding FastEndpoints' serialization hooks directly, and enforces a strict `406 Not Acceptable` for anything else, mirroring MVC's `ReturnHttpNotAcceptable = true`.

### How it works

- **`Config.Serializer.RequestDeserializer`** — inspects the request's `Content-Type`. If it contains `xml`, the body is deserialized with `System.Xml.Serialization.XmlSerializer`; otherwise it falls back to `System.Text.Json`.
- **`Config.Serializer.ResponseSerializer`** — inspects the request's `Accept` header (falling back to the `contentType` FastEndpoints passes in). If XML is requested, the response DTO is serialized with `XmlSerializer` and written with `Content-Type: application/xml`; otherwise JSON is returned as usual.
- **`AcceptHeaderValidator`** (`PreProcessors/AcceptHeaderValidator.cs`) — a global `IGlobalPreProcessor` that runs before every endpoint. If the `Accept` header doesn't match a supported media type (and isn't `*/*` or empty), it short-circuits the pipeline and returns `406 Not Acceptable` without ever reaching the handler.

```csharp
// Program.cs
app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
    c.Versioning.Prefix = "v";
    c.Versioning.PrependToRoute = true;

    c.Endpoints.Configurator = ep =>
    {
        ep.PreProcessor<AcceptHeaderValidator>(Order.Before); // -> 406 for unsupported Accept headers
    };

    c.Serializer.RequestDeserializer = async (req, tDto, jsonSerializerContext, ct) =>
    {
        if (req.ContentType?.Contains("xml") == true)
        {
            using var reader = new StreamReader(req.Body);
            var xml = await reader.ReadToEndAsync(ct);
            var serializer = new XmlSerializer(tDto);
            using var stringReader = new StringReader(xml);
            return serializer.Deserialize(stringReader)!;
        }

        return await JsonSerializer.DeserializeAsync(req.Body, tDto, (JsonSerializerOptions)null!, ct);
    };

    c.Serializer.ResponseSerializer = (rsp, dto, contentType, jsonContext, ct) =>
    {
        var acceptHeader = rsp.HttpContext.Request.Headers.Accept.ToString();
        var wantsXml = acceptHeader.Contains("xml") || contentType?.Contains("xml") == true;

        if (wantsXml)
        {
            rsp.ContentType = "application/xml";
            var serializer = new XmlSerializer(dto!.GetType());
            using var ms = new MemoryStream();
            serializer.Serialize(ms, dto);
            return rsp.Body.WriteAsync(ms.ToArray(), ct).AsTask();
        }

        rsp.ContentType = "application/json";
        return JsonSerializer.SerializeAsync(rsp.Body, dto, dto!.GetType(), (JsonSerializerOptions)null!, ct);
    };
});
```

```csharp
// PreProcessors/AcceptHeaderValidator.cs
public class AcceptHeaderValidator : IGlobalPreProcessor
{
    private static readonly string[] Supported = { "application/json", "application/xml", "text/xml" };

    public Task PreProcessAsync(IPreProcessorContext ctx, CancellationToken ct)
    {
        var accept = ctx.HttpContext.Request.Headers.Accept.ToString();

        var isAcceptable =
            string.IsNullOrEmpty(accept) ||
            accept.Contains("*/*") ||
            Supported.Any(s => accept.Contains(s, StringComparison.OrdinalIgnoreCase));

        if (!isAcceptable && !ctx.HttpContext.ResponseStarted())
        {
            ctx.HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
            return ctx.HttpContext.Response.StartAsync(ct);
        }

        return Task.CompletedTask;
    }
}
```

### DTO requirements for XML support

`XmlSerializer` is far stricter than `System.Text.Json` about the shapes it can (de)serialize. Any request/response DTO that needs to work over XML — which in practice means **all of them**, since content negotiation is global — must follow these rules:

| Requirement | Why |
|---|---|
| **Public parameterless constructor** | `XmlSerializer` instantiates the object directly and populates it via setters; it cannot call a parameterized constructor. |
| **Public read/write (`get; set;`) properties** | `init`-only or get-only properties can't be set after construction and will be silently skipped or throw. |
| **Avoid C# `record` types** | Records default to a primary constructor + `init` accessors — both incompatible. Prefer plain `class` DTOs. |
| **Concrete collection types only** (`List<T>`, `T[]`) | Interface-typed collection properties (`IReadOnlyCollection<T>`, `IEnumerable<T>`, `ICollection<T>`) throw `NotSupportedException: Cannot serialize member ... because it is an interface.` at runtime. |
| **`required` is fine, but still needs a normal setter** | `XmlSerializer` ignores `required` and just uses the public setter — make sure one exists. |

Example — a response DTO written to be XML-safe:

```csharp
public class UserItemResponseDto
{
    public UserItemResponseDto() { } // parameterless ctor

    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public bool EmailConfirmed { get; set; }
    public List<string> Roles { get; set; } = new(); // concrete List<T>, not IReadOnlyCollection<T>
}

public class GetUsersResponseDto
{
    public GetUsersResponseDto() { }

    public List<UserItemResponseDto> Items { get; set; } = new(); // concrete List<T>, not an interface type
}
```

### When to use `[XmlRoot]`, `[XmlElement]`, and `[XmlAttribute]`

By default, `XmlSerializer` needs no annotations at all — it uses the class name as the root element and each public property name as a child element. Reach for these attributes only when the default shape doesn't match what you need:

| Attribute | Use it when... | Default behavior without it |
|---|---|---|
| **`[XmlRoot("Name")]`** | You want to control the **outer/root element name** of the serialized document — e.g. matching a naming convention a consuming system expects, or because the DTO class name itself is awkward as an element name (`GetUsersResponseDto` → `<Users>`). Goes on the **class**, not a property. | The root element is named after the C# class, e.g. `<GetUsersResponseDto>`. |
| **`[XmlElement("Name")]`** | You want a **property serialized under a different element name** than the C# property name — e.g. `Name` property but the XML should say `<ProductName>`; or you need explicit control over element **order** (`XmlSerializer` emits properties in declaration order, so reordering properties in the class changes the XML — `[XmlElement(Order = n)]` can pin it explicitly). | The property is serialized as a child element using its exact C# property name, e.g. `public string Name` → `<Name>`. |
| **`[XmlAttribute("name")]`** | You want a value to appear as an **XML attribute** on the parent element instead of a nested child element — typically for small identifying values (`id`, `code`, `version`) rather than the "body" content of the entity. Common when mirroring an existing XML schema that uses attributes for metadata. | The property is serialized as a child element, never an attribute — this is opt-in only. |

**Rule of thumb:** don't add these speculatively. Start with plain properties (no attributes) and only add `[XmlElement]`/`[XmlAttribute]` when a specific consumer's expected XML shape (or an existing schema/contract you must match) requires renaming, reordering, or promoting a field to an attribute. `[XmlRoot]` is worth adding on every top-level request/response DTO if you want stable, predictable root element names instead of leaking your C# class names into the wire format.

Example — the same `UserItemResponseDto` with annotations applied to rename the root, rename an element, and promote an id to an attribute:

```csharp
[XmlRoot("User")] // root element becomes <User>...</User> instead of <UserItemResponseDto>
public class UserItemResponseDto
{
    public UserItemResponseDto() { }

    [XmlAttribute("id")]              // rendered as an attribute: <User id="...">
    public string UserId { get; set; } = default!;

    [XmlElement("Name")]              // rendered as <Name> instead of <UserName>
    public string UserName { get; set; } = default!;

    public string Email { get; set; } = default!;
    public bool EmailConfirmed { get; set; }
    public List<string> Roles { get; set; } = new();
}
```

which serializes as:

```xml
<User id="abc-123">
  <Name>jdoe</Name>
  <Email>jdoe@example.com</Email>
  <EmailConfirmed>true</EmailConfirmed>
  <Roles>
    <string>Admin</string>
  </Roles>
</User>
```

Also worth knowing: **`[XmlIgnore]`** excludes a property from XML entirely (useful if a property should serialize to JSON but never appear in XML output) — pair it with `[JsonIgnore]` if you need the opposite exclusion for JSON.

### Behavior summary

| Request | Result |
|---|---|
| `Content-Type: application/json` | Body deserialized as JSON |
| `Content-Type: application/xml` or `text/xml` | Body deserialized as XML via `XmlSerializer` |
| `Accept: application/json` (or omitted / `*/*`) | Response serialized as JSON |
| `Accept: application/xml` or `text/xml` | Response serialized as XML via `XmlSerializer` |
| `Accept: <unsupported type>` (e.g. `text/csv`) | **`406 Not Acceptable`** returned immediately by `AcceptHeaderValidator`; handler never runs |

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

1. `<FeatureName>RequestDto.cs` — input DTO
2. `<FeatureName>ResponseDto.cs` — output DTO
3. `<FeatureName>Validation.cs` — a `Validator<TRequest>` with FluentValidation rules
4. `<FeatureName>Endpoint.cs` — the FastEndpoints endpoint: configure route/version/auth/throttling in `Configure()`, implement logic in `HandleAsync()`

This keeps each feature isolated, easy to locate, and easy to delete/replace without touching unrelated code.

# Feature Slices (Vertical Slice Architecture)

Every API operation is a self-contained slice under `VerticalSliceFastEndpoints/Features/<Area>/<FeatureName>/`. A slice owns its endpoint, DTOs and validation; nothing is shared between slices except the data layer (see [data-layer.md](data-layer.md)) and `Core/`.

## Folder layout

```
Features/
└── Product/                      # <Area>: one folder per aggregate (Auth, Product, Users)
    └── CreateProduct/            # <FeatureName>: verb + noun
        ├── CreateProductEndpoint.cs
        ├── CreateProductRequestDto.cs
        ├── CreateProductResponseDto.cs
        └── CreateProductValidation.cs
```

- Namespace matches the folder (`Features.Product.CreateProduct`); `GlobalUsings.cs` supplies nearly all `using`s, so new files rarely need any.
- Omit files a slice doesn't need (e.g. `DeleteProduct` has no request/response/validator; `GetProduct` has no request).
- Types used by one slice only stay in that slice (e.g. `ProductItemResponseDto` inside `GetProducts`).

## Endpoint

Pick the base class by what the slice needs:

| Base class | Use when |
|---|---|
| `Endpoint<TReq, TRes>` | Request and response DTOs, no entity mapping (`GetProducts`, `Login`) |
| `EndpointWithMapping<TReq, TRes, TEntity>` | Request maps to/from an entity (`CreateProduct`, `UploadImage`); implement `MapToEntity`/`MapFromEntity` |
| `EndpointWithoutRequest` | Only route data, no body/query DTO (`DeleteProduct`) |

Inject `ApplicationDbContext` (or Identity managers) through the constructor into a `private readonly` field. Alias entities that clash with the area namespace: `using Entity = VerticalSliceFastEndpoints.Entities.Product;`.

### `Configure()`

Order and contents used by existing slices:

```csharp
public override void Configure()
{
    PreProcessor<PreProcessorLogging<CreateProductRequestDto>>();
    Post("/products");                       // route only; /api/v{n} is added globally
    Version(1);
    Roles(ApplicationRole.Admin);            // or AllowAnonymous() for public reads
    Throttle(hitLimit: 20, durationSeconds: 60);

    Description(x => x
        .WithTags("Products")
        .WithDescription("Creates a new product in the system.")
        .Produces<CreateProductResponseDto>(201)
        .ProducesProblem(400));
}
```

- Add `PreProcessorLogging<TReq>` to each endpoint that takes a request DTO.
- Write operations require a role from `ApplicationRole` (never a string literal) and a `Throttle`; anonymous read endpoints call `AllowAnonymous()`.
- `Description` must list every status code the handler can return (`Produces`/`ProducesProblem`). Use `.Accepts<T>("multipart/form-data")` for file uploads.
- `WithTags` groups the operation in Scalar (`Products`, `Authentication`, ...).

### `HandleAsync()`

- Read route values with `Route<Guid>("id")`; the current user id with `User.FindFirstValue(ClaimTypes.NameIdentifier)`.
- Use `AsNoTracking()` for read-only queries; paginate with `Skip`/`Take` and return the page envelope (`Items`, `PageNumber`, `PageSize`, `TotalCount`).
- Respond through `Send.*` (`OkAsync`, `CreatedAtAsync`, `NoContentAsync`, `NotFoundAsync`) and always pass the `CancellationToken`.
- Typical status codes: `201` create, `200` read/update, `204` delete, `404` missing entity, `400` validation/business failure.
- Persist with `SaveChangesAsync(ct)`; delete with `Remove` (it becomes a soft delete for `ISoftDeletable`). Don't set audit fields yourself.

## DTOs

- Plain classes with an explicit public parameterless constructor, settable properties and defaults (`string.Empty`, `= 1`) so the XmlSerializer and query-string binding both work.
- Name them `<Feature>RequestDto` / `<Feature>ResponseDto`; list items use `<Thing>ItemResponseDto`.
- Add `[XmlRoot]`/`[XmlElement]` only when an XML schema requires it.

## Validation

One FluentValidation `Validator<TReq>` per request DTO, named `<Feature>Validation`, with a `.WithMessage(...)` on every rule:

```csharp
public class CreateProductValidation : Validator<CreateProductRequestDto>
{
    public CreateProductValidation()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(50).WithMessage("Title cannot exceed 50 characters.");
    }
}
```

FastEndpoints discovers validators automatically; limits should mirror the column lengths in `Data/Configurations/`.

## Adding a new slice

1. Create `Features/<Area>/<FeatureName>/` with the files above.
2. Configure route, version, auth, throttle and OpenAPI description.
3. Add validation for every constrained request field.
4. If the API version is new, see [api-configuration.md](api-configuration.md).

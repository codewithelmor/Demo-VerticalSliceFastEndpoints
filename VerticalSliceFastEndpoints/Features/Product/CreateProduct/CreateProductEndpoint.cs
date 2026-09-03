using VerticalSliceFastEndpoints;
using VerticalSliceFastEndpoints.Data;
using Entity = VerticalSliceFastEndpoints.Entities.Product;

namespace Features.Product.CreateProduct;

public class CreateProductEndpoint : EndpointWithMapping<CreateProductRequest, CreateProductResponse, Entity>
{
    private readonly ApplicationDbContext _context;

    public CreateProductEndpoint(ApplicationDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        PreProcessor<PreProcessorLogging<CreateProductRequest>>();
        Post("/products");
        Version(1);
        //AllowAnonymous();

        // 1. Remove AllowAnonymous(); and enforce JWT Bearer validation
        // (This requires an "Authorization: Bearer <token>" header)
        // AuthSchemes("Bearer"); // Optional: Use only if you have multiple schemas configured

        // 2. Enforce Role Authorization
        Roles(SD.Role_Admin); // Add the specific roles allowed to access this slice

        Throttle(hitLimit: 20, durationSeconds: 60);

        Description(x => x
            .WithTags("Products")
            .Produces<CreateProductResponse>(201)
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        // 1. Map request DTO to your Product domain entity
        var product = MapToEntity(req);

        // 2. Add the product to the DbContext and save changes to the database
        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);

        // 3. Map the saved entity back to the response DTO
        var response = MapFromEntity(product);

        // 4. Return HTTP 201 Created with the location header
        await Send.CreatedAtAsync("/products/{id}", new { id = response.Id }, response, cancellation: ct);
    }

    public override Entity MapToEntity(CreateProductRequest req)
    {
        return new Entity(req.Title, req.Description);
    }
    
    public override CreateProductResponse MapFromEntity(Entity entity)
    {
        return new CreateProductResponse { Id = entity.Id, Title = entity.Title, Description = entity.Description, CreatedDate = entity.CreatedDate, UpdatedDate = entity.UpdatedDate };
    }
}
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VerticalSliceFastEndpoints;
using VerticalSliceFastEndpoints.Data;
using Entity = VerticalSliceFastEndpoints.Entities.Product;

namespace Features.Product.UpdateProduct;

public class UpdateProductEndpoint : EndpointWithMapping<UpdateProductRequest, UpdateProductResponse, Entity>
{
    private readonly ApplicationDbContext _context;

    public UpdateProductEndpoint(ApplicationDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        PreProcessor<PreProcessorLogging<UpdateProductRequest>>();
        // {id} will bind automatically to the 'Id' property inside UpdateProductRequest
        Put("/products/{id}");
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
            .WithDescription("Update an existing product by its ID. Requires Admin role.")
            .Produces<UpdateProductResponse>(201)
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        // Extract the {id} parameter safely directly from the route path
        var id = Route<Guid>("id");

        // 1. Fetch the product using the extracted ID
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // 2. Perform updates from the body payload
        product.Title = req.Title;
        product.Description = req.Description;
        product.UpdatedDate = DateTime.UtcNow;

        // 2. Update the product to the DbContext and save changes to the database
        _context.Products.Update(product);
        await _context.SaveChangesAsync(ct);

        // 3. Map the saved entity back to the response DTO
        var response = MapFromEntity(product);

        // 4. Returns HTTP 200 with the response object in the body
        await Send.OkAsync(response, ct);
    }

    public override Entity MapToEntity(UpdateProductRequest req)
    {
        // Standard ASP.NET Core approach
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        return new Entity(req.Title, req.Description, updatedByUserId: userId);
    }
    
    public override UpdateProductResponse MapFromEntity(Entity entity)
    {
        return new UpdateProductResponse { Id = entity.Id, Title = entity.Title, Description = entity.Description };
    }
}
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VerticalSliceFastEndpoints;
using VerticalSliceFastEndpoints.Data;

namespace Features.Product.DeleteProduct;

public class DeleteProductEndpoint : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _context;

    public DeleteProductEndpoint(ApplicationDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Delete("/products/{id}");
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
            .WithDescription("Deletes a product by its ID. This is a soft delete operation.")
            .Produces(204) // No Content
            .ProducesProblem(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Extract the {id} parameter safely directly from the route path
        var id = Route<Guid>("id");

        // Standard ASP.NET Core approach
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // 1. Fetch the product using the extracted ID
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        // 2. Return 404 if the product doesn't exist or is already soft-deleted
        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // 3. Apply soft-delete timestamp instead of calling _context.Products.Remove(product)
        product.DeletedDate = DateTime.UtcNow;
        product.DeletedByUserId = userId;

        // 4. Persist changes to the database
        await _context.SaveChangesAsync(ct);

        // 5. Return HTTP 204 No Content as standard for delete operations
        await Send.NoContentAsync(ct);
    }
}

using Microsoft.EntityFrameworkCore;
using VerticalSliceFastEndpoints.Data;
using VerticalSliceFastEndpoints.Features.Product.GetProducts;

namespace Features.Product.GetProduct;

public class GetProductEndpoint : EndpointWithoutRequest<GetProductResponseDto>
{
    private readonly ApplicationDbContext _context;

    public GetProductEndpoint(ApplicationDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Get("/products/{id}");
        Version(1);
        AllowAnonymous();

        Description(x => x
            .WithTags("Products")
            .WithDescription("Get a single product by its ID.")
            .Produces<GetProductResponseDto>(200)
            .ProducesProblem(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Extract the {id} parameter safely from the route path
        var id = Route<Guid>("id");

        // 1. Fetch the single product using the extracted ID
        var product = await _context.Products
            .AsNoTracking() // Optimizes performance for read-only tracking
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        // 2. Return 404 if the product doesn't exist (or is soft-deleted)
        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // 3. Map to response object
        var response = new GetProductResponseDto
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            FileContent = Convert.ToBase64String(product.ImageContent ?? new byte[0]),
            FileName = product.ImageFileName
        };

        // 4. Returns HTTP 200 with the response object in the body
        await Send.OkAsync(response, ct);
    }
}

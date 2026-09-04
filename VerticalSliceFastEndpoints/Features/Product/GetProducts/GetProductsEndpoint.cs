using Microsoft.EntityFrameworkCore;
using VerticalSliceFastEndpoints.Data;
using VerticalSliceFastEndpoints.Features.Product.GetProducts;

namespace Features.Product.GetProducts;

public class GetProductsEndpoint : Endpoint<GetProductsRequest, GetProductsResponse>
{
    private readonly ApplicationDbContext _context;

    public GetProductsEndpoint(ApplicationDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Get("/products");
        Version(1);
        AllowAnonymous();

        Description(x => x
            .WithTags("Products")
            .WithDescription("Retrieves a paginated list of products from the database.")
            .Produces<GetProductsResponse>(200)
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(GetProductsRequest req, CancellationToken ct)
    {
        // 1. Query the database with pagination bounds
        var items = await _context.Products
            .AsNoTracking() // Optimizes performance for read-only queries
            .OrderByDescending(p => p.CreatedDate)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new ProductItemResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate
            })
            .ToListAsync(ct);

        // 2. Fetch total count for metadata mapping
        var totalCount = await _context.Products.CountAsync(ct);

        // 3. Populate and return response envelope
        var response = new GetProductsResponse
        {
            Items = items,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalCount = totalCount
        };

        await Send.OkAsync(response, ct);
    }
}

using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VerticalSliceFastEndpoints;
using VerticalSliceFastEndpoints.Data;
using Entity = VerticalSliceFastEndpoints.Entities.Product;

namespace Features.Product.UploadImage;

public class UploadImageEndpoint : EndpointWithMapping<UploadImageRequestDto, UploadImageResponseDto, Entity>
{
    private readonly ApplicationDbContext _context;

    public UploadImageEndpoint(ApplicationDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        PreProcessor<PreProcessorLogging<UploadImageRequestDto>>();
        Post("/products/{id}");
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
            .WithDescription("Uploads an image for a specific product.Requires Admin role.")
            .Accepts<UploadImageRequestDto>("multipart/form-data") // For Swagger UI display
            .Produces<UploadImageResponseDto>(201)
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(UploadImageRequestDto req, CancellationToken ct)
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

        // 3. Extract file bytes cleanly without memory bloating via MemoryStream
        using var memoryStream = new MemoryStream();
        await req.File.CopyToAsync(memoryStream, ct);
        byte[] imageBytes = memoryStream.ToArray();
        string fileContent = Convert.ToBase64String(imageBytes);

        // 4. Update your domain model properties (assuming these fields exist on your Product Entity)
        // Ensure your DbContext mapping rules or Entity has: public byte[]? ImageContent { get; set; }
        product.ImageContent = imageBytes;
        product.ImageFileName = req.File.FileName;
        product.ImageUploadedDate = DateTime.Now;
        product.ImageUploadedByUserId = userId;

        // 5. Commit changes to SQL Server
        await _context.SaveChangesAsync(ct);

        // 6. Map to Response and return success status
        var response = new UploadImageResponseDto
        {
            ProductId = product.Id,
            FileName = req.File.FileName,
            FileContent = fileContent
        };

        // 7. Returns HTTP 200 with the response object in the body
        await Send.OkAsync(response, ct);
    }
}
using VerticalSliceFastEndpoints.Entities;

namespace Features.Product.CreateProduct;

public class CreateProductEndpoint : EndpointWithMapping<CreateProductRequest, CreateProductResponse, Product>
{
    public override void Configure()
    {
        PreProcessor<PreProcessorLogging<CreateProductRequest>>();
        Post("/products");
        AllowAnonymous();

        Throttle(hitLimit: 20, durationSeconds: 60);

        Description(x => x
            .Produces<CreateProductResponse>(201)
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var Product = MapToEntity(req);
        var response = MapFromEntity(Product);

        await Send.CreatedAtAsync("/products/{id}", new { id = Product.Id }, response, cancellation: ct);
    }

    public override Product MapToEntity(CreateProductRequest req)
    {
        return new Product(req.Title, req.Description);
    }
    
    public override CreateProductResponse MapFromEntity(Product entity)
    {
        return new CreateProductResponse { Id = entity.Id };
    }
}
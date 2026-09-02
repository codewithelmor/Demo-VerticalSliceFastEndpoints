using VerticalSliceFastEndpoints.Entities;

namespace Features.Category.CreateCategory;

public class CreateCategoryEndpoint : EndpointWithMapping<CreateCategoryRequest, CreateCategoryResponse, CategoryItem>
{
    public override void Configure()
    {
        PreProcessor<PreProcessorLogging<CreateCategoryRequest>>();
        Post("/categories");
        AllowAnonymous();

        Throttle(hitLimit: 20, durationSeconds: 60);

        Description(x => x
            .Produces<CreateCategoryResponse>(201)
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        var categoryItem = MapToEntity(req);
        var response = MapFromEntity(categoryItem);

        await Send.CreatedAtAsync("/categories/{id}", new { id = categoryItem.Id }, response, cancellation: ct);
    }

    public override CategoryItem MapToEntity(CreateCategoryRequest req)
    {
        return new CategoryItem(req.Title, req.Description);
    }
    
    public override CreateCategoryResponse MapFromEntity(CategoryItem entity)
    {
        return new CreateCategoryResponse { Id = entity.Id };
    }
}
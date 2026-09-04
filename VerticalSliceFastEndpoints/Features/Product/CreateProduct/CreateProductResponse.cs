namespace Features.Product.CreateProduct;

public class CreateProductResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
}
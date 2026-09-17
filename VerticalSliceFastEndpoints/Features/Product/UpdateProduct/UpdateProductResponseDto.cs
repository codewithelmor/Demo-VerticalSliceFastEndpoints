namespace Features.Product.UpdateProduct;

public class UpdateProductResponseDto
{
    public UpdateProductResponseDto() { }

    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
}
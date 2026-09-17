namespace  Features.Product.UpdateProduct;

public class UpdateProductRequestDto
{
    public UpdateProductRequestDto() { }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
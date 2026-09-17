namespace  Features.Product.CreateProduct;

public class CreateProductRequestDto
{
    public CreateProductRequestDto() { }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
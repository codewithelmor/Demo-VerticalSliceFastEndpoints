namespace VerticalSliceFastEndpoints.Features.Product.GetProducts
{
    public class ProductItemResponseDto
    {
        public ProductItemResponseDto() { }

        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? FileName { get; set; } = string.Empty;
        public string? FileContent { get; set; } = string.Empty;
    }
}

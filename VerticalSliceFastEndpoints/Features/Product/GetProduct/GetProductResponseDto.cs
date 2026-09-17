namespace VerticalSliceFastEndpoints.Features.Product.GetProducts
{
    public class GetProductResponseDto
    {
        public GetProductResponseDto() { }

        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string? FileName { get; set; } = string.Empty;
        public string? FileContent { get; set; } = string.Empty;
    }
}

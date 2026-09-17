namespace VerticalSliceFastEndpoints.Features.Product.GetProducts
{
    public class GetProductsResponse
    {
        public GetProductsResponse() { }

        public List<ProductItemResponseDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}

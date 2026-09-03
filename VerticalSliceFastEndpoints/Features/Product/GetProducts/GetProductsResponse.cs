namespace VerticalSliceFastEndpoints.Features.Product.GetProducts
{
    public class GetProductsResponse
    {
        public IReadOnlyCollection<ProductItemResponse> Items { get; set; } = Array.Empty<ProductItemResponse>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}

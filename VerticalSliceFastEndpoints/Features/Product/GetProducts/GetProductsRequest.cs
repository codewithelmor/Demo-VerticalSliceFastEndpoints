namespace VerticalSliceFastEndpoints.Features.Product.GetProducts
{
    public class GetProductsRequest
    {
        // Sets defaults if the query string parameter is omitted by the client
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

namespace Features.Product.GetProducts
{
    public class GetProductsRequestDto
    {
        public GetProductsRequestDto() { }

        // Sets defaults if the query string parameter is omitted by the client
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

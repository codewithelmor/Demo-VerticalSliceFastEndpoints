namespace VerticalSliceFastEndpoints.Features.User.GetUsers
{
    public class GetUsersResponse
    {
        public IReadOnlyCollection<UserItemResponse> Items { get; set; } = Array.Empty<UserItemResponse>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}

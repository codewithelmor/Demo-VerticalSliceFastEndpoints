namespace VerticalSliceFastEndpoints.Features.User.GetUsers
{
    public class GetUsersResponseDto
    {
        public GetUsersResponseDto() { }

        public List<UserItemResponseDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}

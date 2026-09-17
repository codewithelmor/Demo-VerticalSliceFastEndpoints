namespace Features.User.GetUsers
{
    public class GetUsersRequestDto
    {
        public GetUsersRequestDto() { }

        // Sets defaults if the query string parameter is omitted by the client
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

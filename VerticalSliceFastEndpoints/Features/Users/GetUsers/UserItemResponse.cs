namespace VerticalSliceFastEndpoints.Features.User.GetUsers
{
    public class UserItemResponse
    {
        public string UserId { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public bool EmailConfirmed { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}

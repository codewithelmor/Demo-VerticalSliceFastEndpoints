namespace VerticalSliceFastEndpoints.Features.Auth.Login
{
    public class LoginRequestDto
    {
        public LoginRequestDto() { }

        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!; // NOTE: For demo only - NOT encrypted
    }
}

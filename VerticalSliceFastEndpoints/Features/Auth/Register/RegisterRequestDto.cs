namespace VerticalSliceFastEndpoints.Features.Auth.Register
{
    public class RegisterRequestDto
    {
        public RegisterRequestDto() { }

        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!; // NOTE: For demo only - NOT encrypted

        public string Role { get; set; } = ApplicationRole.Customer;
    }
}

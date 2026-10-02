namespace VerticalSliceFastEndpoints.Features.Auth.Register
{
    public class RegisterEndpoint : Endpoint<RegisterRequestDto, RegisterResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterEndpoint(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public override void Configure()
        {
            Post("/auth/register");
            Version(1);
            AllowAnonymous();
            Description(x => x
                .WithTags("Authentication")
                .Produces<RegisterResponseDto>(200)
                .Produces<RegisterResponseDto>(400)
                .Produces<RegisterResponseDto>(404));
        }

        public override async Task HandleAsync(RegisterRequestDto req, CancellationToken ct)
        {
            var existingUser = await _userManager.FindByNameAsync(req.Email);
            if (existingUser != null)
            {
                await Send.ResultAsync(Results.BadRequest(new RegisterResponseDto
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = new List<string> { "Username already exists" }
                }));
            }

            // Seed roles dynamically if they don't exist
            if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
            }

            var newUser = new ApplicationUser
            {
                UserName = req.Email,
                Email = req.Email,
                NormalizedEmail = req.Email.ToUpper()
            };

            var result = await _userManager.CreateAsync(newUser, req.Password);
            if (!result.Succeeded)
            {
                await Send.ResultAsync(Results.BadRequest(new RegisterResponseDto
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = result.Errors.Select(e => e.Description).ToList()
                }));
            }

            // Handle Role Assignment
            if (string.Equals(req.Role, SD.Role_Admin, StringComparison.OrdinalIgnoreCase))
            {
                await _userManager.AddToRoleAsync(newUser, SD.Role_Admin);
            }
            else
            {
                await _userManager.AddToRoleAsync(newUser, SD.Role_Customer);
            }

            await Send.OkAsync(new RegisterResponseDto
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = "Registration processing completed successfully."
            }, cancellation: ct);
        }
    }
}

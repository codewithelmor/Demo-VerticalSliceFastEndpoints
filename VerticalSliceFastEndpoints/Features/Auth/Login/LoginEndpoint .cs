namespace VerticalSliceFastEndpoints.Features.Auth.Login
{
    public class LoginEndpoint : Endpoint<LoginRequestDto, LoginResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public LoginEndpoint(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public override void Configure()
        {
            Post("/auth/login");
            Version(1);
            AllowAnonymous();
            Description(x => x
                .WithTags("Authentication")
                .Produces<LoginResponseDto>(200)
                .Produces<LoginResponseDto>(400)
                .Produces<LoginResponseDto>(404));
        }

        public override async Task HandleAsync(LoginRequestDto req, CancellationToken ct)
        {
            var user = await _userManager.FindByNameAsync(req.Email);
            if (user == null)
            {
                await SendBadRequestAsync("Username or password is incorrect", ct);
                return;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, req.Password, false);
            if (!result.Succeeded)
            {
                await SendBadRequestAsync("Username or password is incorrect", ct);
                return;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Customer"; // Fallback to your Role.Customer string

            // Create JWT Token
            var key = Encoding.ASCII.GetBytes(_configuration["ApiSettings:Secret"]!);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, role),
                new Claim("API", "Product API v1")
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            await Send.OkAsync(new LoginResponseDto
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = new
                {
                    Token = tokenString,
                    Email = req.Email
                },
            }, cancellation: ct);
        }

        private async Task SendBadRequestAsync(string errorMessage, CancellationToken ct)
        {
            await Send.ResultAsync(Results.BadRequest(new RegisterResponseDto
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessages = new List<string> { errorMessage }
            }));
        }
    }

}

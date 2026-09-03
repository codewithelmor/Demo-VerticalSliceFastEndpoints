namespace VerticalSliceFastEndpoints.Features.Auth.Register
{
    public class RegisterValidation : Validator<RegisterRequest>
    {
        public RegisterValidation()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

            RuleFor(x => x.Password)
                       .NotEmpty().WithMessage("Password is required.")
                       // NIST Standards champion length: 8-12 character minimums (12+ is highly recommended)
                       .MinimumLength(12).WithMessage("Password must be at least 12 characters long.")
                       .MaximumLength(128).WithMessage("Password cannot exceed 128 characters.")
                       // Complexity checks (Ensures basic entropy defense)
                       .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                       .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                       .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                       .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.");
        }
    }

}

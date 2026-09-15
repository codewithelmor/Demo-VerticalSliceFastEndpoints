namespace Features.User.GetUsers;

public class GetUsersValidation : Validator<GetUsersRequest>
{
    public GetUsersValidation()
    {
        // Enforce that PageNumber must be 1 or higher
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");

        // Enforce that PageSize must be between 1 and 100 inclusive
        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100 rows per request.");
    }
}

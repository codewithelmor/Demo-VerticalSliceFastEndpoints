namespace Features.Category.CreateCategory;

public class CreateCategoryValidation : Validator<CreateCategoryRequest>
{
    public CreateCategoryValidation()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(1).WithMessage("Title must be at least 1 character long.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

    }
}
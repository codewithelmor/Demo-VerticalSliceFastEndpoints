namespace Features.Product.CreateProduct;

public class CreateProductValidation : Validator<CreateProductRequest>
{
    public CreateProductValidation()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(1).WithMessage("Title must be at least 1 character long.")
            .MaximumLength(50).WithMessage("Title cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(100).WithMessage("Description cannot exceed 100 characters.");

    }
}
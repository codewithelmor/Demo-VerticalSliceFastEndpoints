namespace Features.Product.UploadImage;

public class UploadImageValidation : Validator<UploadImageRequest>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

    public UploadImageValidation()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("A file must be uploaded.")
            .DependentRules(() =>
            {
                // Enforce minimum size rule
                RuleFor(x => x.File.Length)
                    .GreaterThan(0)
                    .WithMessage("The uploaded file cannot be empty.");

                // Enforce maximum size limit to protect database memory cache
                RuleFor(x => x.File.Length)
                    .LessThanOrEqualTo(MaxFileSizeInBytes)
                    .WithMessage($"File size exceeds the maximum limit of {MaxFileSizeInBytes / 1024 / 1024} MB.");

                // Enforce clean allowed file extension rules
                RuleFor(x => x.File.FileName)
                    .Must(HasAllowedExtension)
                    .WithMessage($"Invalid file type. Only {string.Join(", ", AllowedExtensions)} images are allowed.");
            });
    }

    private static bool HasAllowedExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }
}
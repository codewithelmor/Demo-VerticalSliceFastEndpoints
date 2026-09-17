namespace  Features.Product.UploadImage;

public class UploadImageRequestDto
{
    public UploadImageRequestDto() { }

    public IFormFile File { get; set; } = null!;
}
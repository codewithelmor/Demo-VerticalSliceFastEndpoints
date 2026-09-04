namespace Features.Product.UploadImage;

public class UploadImageResponse
{
    public Guid ProductId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileContent { get; set; } = string.Empty;
}
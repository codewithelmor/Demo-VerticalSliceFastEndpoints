namespace VerticalSliceFastEndpoints.Entities;

public partial class Product
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string CreatedByUserId { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedByUserId { get; set; } = null;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDate { get; set; }
    public string? DeletedByUserId { get; set; } = null;
    public byte[]? ImageContent { get; set; }
    public string? ImageFileName { get; set; }
    public DateTime? ImageUploadedDate { get; set; }
    public string? ImageUploadedByUserId { get; set; } = null;

    public Product() { }

    public Product(string title, string description, string? createdByUserId = null, string? updatedByUserId = null, string? deletedByUserId = null, byte[]? imageContent = null, string? imageFileName = null, string? imageUploadedByUserId = null)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        CreatedDate = DateTime.Now;
        CreatedByUserId = createdByUserId!;
        UpdatedByUserId = updatedByUserId;
        DeletedByUserId = deletedByUserId;
        ImageContent = imageContent;
        ImageFileName = imageFileName;
        ImageUploadedByUserId = imageUploadedByUserId;
    }
}

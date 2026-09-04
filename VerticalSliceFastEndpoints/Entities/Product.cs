using System.ComponentModel.DataAnnotations.Schema;

namespace VerticalSliceFastEndpoints.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
    public byte[]? ImageContent { get; set; }
    public string? ImageFileName { get; set; }
    public DateTime? ImageUploadedDate { get; set; }

    // Foreign Key Property
    public string CreatedByUserId { get; set; }

    // Navigation Property
    [ForeignKey(nameof(CreatedByUserId))]
    public virtual ApplicationUser? CreatedByUser { get; set; }

    // Foreign Key Property
    public string? UpdatedByUserId { get; set; } = null;

    // Navigation Property
    [ForeignKey(nameof(UpdatedByUserId))]
    public virtual ApplicationUser? UpdatedByUser { get; set; }

    // Foreign Key Property
    public string? DeletedByUserId { get; set; } = null;

    // Navigation Property
    [ForeignKey(nameof(DeletedByUserId))]
    public virtual ApplicationUser? DeletedByUser { get; set; }

    // Foreign Key Property
    public string? ImageUploadedByUserId { get; set; } = null;

    // Navigation Property
    [ForeignKey(nameof(ImageUploadedByUserId))]
    public virtual ApplicationUser? ImageUploadedByUser { get; set; }

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
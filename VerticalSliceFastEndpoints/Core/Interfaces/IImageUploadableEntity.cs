namespace VerticalSliceFastEndpoints.Core.Interfaces
{
    public interface IImageUploadableEntity
    {
        byte[]? ImageContent { get; set; }
        string? ImageFileName { get; set; }
        DateTime? ImageUploadedDate { get; set; }
        string? ImageUploadedByUserId { get; set; }
    }
}

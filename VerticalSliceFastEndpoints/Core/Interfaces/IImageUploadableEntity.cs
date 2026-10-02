namespace VerticalSliceFastEndpoints.Core.Interfaces
{
    public interface IImageUploadableEntity
    {
        DateTime? ImageUploadedDate { get; set; }
        string? ImageUploadedByUserId { get; set; }
    }
}

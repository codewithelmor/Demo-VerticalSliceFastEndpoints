namespace VerticalSliceFastEndpoints.Core.Interfaces
{
    public interface IAuditableEntity
    {
        DateTime CreatedDate { get; set; }
        string CreatedByUserId { get; set; }
        DateTime? UpdatedDate { get; set; }
        string? UpdatedByUserId { get; set; }
    }
}

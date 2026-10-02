namespace VerticalSliceFastEndpoints.Core.Interfaces
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedDate { get; set; }
        string? DeletedByUserId { get; set; }
    }
}

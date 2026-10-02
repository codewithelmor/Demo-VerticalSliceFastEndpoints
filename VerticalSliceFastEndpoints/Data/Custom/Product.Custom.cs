namespace VerticalSliceFastEndpoints.Entities
{
    public partial class Product : IAuditableEntity, ISoftDeletable, IImageUploadableEntity
    {
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual ApplicationUser? CreatedByUser { get; set; }

        [ForeignKey(nameof(UpdatedByUserId))]
        public virtual ApplicationUser? UpdatedByUser { get; set; }

        [ForeignKey(nameof(DeletedByUserId))]
        public virtual ApplicationUser? DeletedByUser { get; set; }

        [ForeignKey(nameof(ImageUploadedByUserId))]
        public virtual ApplicationUser? ImageUploadedByUser { get; set; }
    }
}

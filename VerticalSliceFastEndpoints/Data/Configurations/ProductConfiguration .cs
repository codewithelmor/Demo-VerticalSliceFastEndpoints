using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerticalSliceFastEndpoints.Entities;

namespace VerticalSliceFastEndpoints.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Set the primary key
            builder.HasKey(p => p.Id);

            // Configure Title with a max length of 50 and make it required
            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(50);

            // Configure Description with a max length of 100 and make it required
            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(100);

            // Ensure dates are mapped correctly
            builder.Property(p => p.CreatedDate)
                .IsRequired();

            builder.Property(p => p.UpdatedDate)
                .IsRequired(false);

            builder.Property(p => p.DeletedDate)
                .IsRequired(false);

            // Optional: Filter out soft-deleted items by default
            builder.HasQueryFilter(p => p.DeletedDate == null);
        }
    }
}

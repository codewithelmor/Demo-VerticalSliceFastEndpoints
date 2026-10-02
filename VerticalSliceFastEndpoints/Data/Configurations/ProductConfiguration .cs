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

            // Configure ImageFileName with a max length of 50 and make it optional
            builder.Property(p => p.ImageFileName)
                .HasMaxLength(50);

            // Ensure dates are mapped correctly
            builder.Property(p => p.CreatedDate)
                .IsRequired();

            builder.Property(p => p.UpdatedDate)
                .IsRequired(false);

            builder.Property(p => p.DeletedDate)
                .IsRequired(false);

            // Optional: Filter out soft-deleted items by default
            builder.HasQueryFilter(p => p.DeletedDate == null);

            builder.HasOne(p => p.CreatedByUser)        // Product has one User
                .WithMany()                          // User can have many Products
                .HasForeignKey(p => p.CreatedByUserId) // Foreign Key definition
                .OnDelete(DeleteBehavior.Restrict);   // Prevents cascading deletes if a user is deleted

            builder.HasOne(p => p.UpdatedByUser)        // Product has one User
                .WithMany()                          // User can have many Products
                .HasForeignKey(p => p.UpdatedByUserId) // Foreign Key definition
                .OnDelete(DeleteBehavior.Restrict);   // Prevents cascading deletes if a user is deleted

            builder.HasOne(p => p.DeletedByUser)        // Product has one User
                .WithMany()                          // User can have many Products
                .HasForeignKey(p => p.DeletedByUserId) // Foreign Key definition
                .OnDelete(DeleteBehavior.Restrict);   // Prevents cascading deletes if a user is deleted

            builder.HasOne(p => p.ImageUploadedByUser)        // Product has one User
                .WithMany()                          // User can have many Products
                .HasForeignKey(p => p.ImageUploadedByUserId) // Foreign Key definition
                .OnDelete(DeleteBehavior.Restrict);   // Prevents cascading deletes if a user is deleted
        }
    }
}

namespace VerticalSliceFastEndpoints.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ICurrentUserService _currentUserService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAuditInformation();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplyAuditInformation()
        {
            var currentUserId = _currentUserService?.UserId ?? "System";
            var now = DateTime.Now;

            // 1) Soft delete: turn hard deletes into updates and stamp Deleted*
            foreach (var entry in ChangeTracker.Entries<ISoftDeletable>().Where(e => e.State == EntityState.Deleted).ToList())
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedDate = now;
                entry.Entity.DeletedByUserId = currentUserId;
            }

            // 2) Created*/Modified*
            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedDate = now;
                    entry.Entity.CreatedByUserId = currentUserId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedDate = now;
                    entry.Entity.UpdatedByUserId = currentUserId;
                }
            }
        }
    }
}

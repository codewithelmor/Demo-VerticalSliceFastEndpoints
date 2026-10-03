namespace VerticalSliceFastEndpoints.Data.Initialization;

public sealed class ApplicationDbContextInitializer(
    ApplicationDbContext db,
    IOptions<DatabaseInitializerOptions> options,
    ILogger<ApplicationDbContextInitializer> logger) : IDatabaseInitializer
{
    private readonly DatabaseInitializerOptions _options = options.Value;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Migrate && !_options.Seed)
        {
            logger.LogInformation("Database initialization disabled; skipping.");
            return;
        }

        if (_options.Migrate)
            await MigrateAsync(cancellationToken);

        if (_options.Seed)
            await SeedAsync(cancellationToken);
    }

    private async Task MigrateAsync(CancellationToken ct)
    {
        var pending = (await db.Database.GetPendingMigrationsAsync(ct)).ToList();

        if (pending.Count == 0)
        {
            logger.LogInformation("Database is up to date; no pending migrations.");
            return;
        }

        logger.LogInformation(
            "Applying {Count} pending migration(s): {Migrations}",
            pending.Count, string.Join(", ", pending));

        // Migrate uses the configured execution strategy (e.g. EnableRetryOnFailure) internally.
        await db.Database.MigrateAsync(ct);

        logger.LogInformation("Migrations applied.");
    }

    private async Task SeedAsync(CancellationToken ct)
    {
        // Must be idempotent: safe to run on every startup and from multiple instances.
        // Example (replace with your entities):

        string[] defaultRoles = ApplicationRole.AllRoles;
        var existing = await db.Roles.Select(r => r.Name).ToListAsync(ct);
        var missing = defaultRoles.Except(existing).Select(n => new IdentityRole { Name = n, NormalizedName = n.ToUpper() }).ToList();

        if (missing.Count > 0)
        {
            db.Roles.AddRange(missing);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded {Count} role(s).", missing.Count);
        }

        await Task.CompletedTask;
    }
}

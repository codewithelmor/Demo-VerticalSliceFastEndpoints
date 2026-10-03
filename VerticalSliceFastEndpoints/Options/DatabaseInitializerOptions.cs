namespace VerticalSliceFastEndpoints.Options;

public sealed class DatabaseInitializerOptions
{
    public const string SectionName = "DatabaseInitializer";

    /// <summary>Apply pending EF Core migrations. Keep false in production; use bundles/scripts in CI/CD.</summary>
    public bool Migrate { get; set; }

    /// <summary>Run idempotent seeding.</summary>
    public bool Seed { get; set; }

    /// <summary>Exit after initialization (migration job / init container mode).</summary>
    public bool ExitAfterInitialization { get; set; }
}

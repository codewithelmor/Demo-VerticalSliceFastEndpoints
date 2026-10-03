namespace VerticalSliceFastEndpoints.Data.Initialization;

public static class DatabaseInitializerExtensions
{
    public static IServiceCollection AddDatabaseInitializer(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseInitializerOptions>()
            .Bind(configuration.GetSection(DatabaseInitializerOptions.SectionName));

        services.AddScoped<IDatabaseInitializer, ApplicationDbContextInitializer>();
        return services;
    }

    /// <summary>
    /// Runs initialization before the app starts serving requests.
    /// Returns true if the host should exit afterwards (migration job mode).
    /// </summary>
    public static async Task<bool> InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        await initializer.InitializeAsync(app.Lifetime.ApplicationStopping);

        return scope.ServiceProvider
            .GetRequiredService<IOptions<DatabaseInitializerOptions>>()
            .Value.ExitAfterInitialization;
    }
}

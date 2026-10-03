namespace VerticalSliceFastEndpoints.Data.Initialization;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

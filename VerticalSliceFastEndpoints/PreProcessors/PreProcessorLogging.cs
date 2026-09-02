namespace VerticalSliceFastEndpoints.PreProcessors;

public class PreProcessorLogging<TRequest> : IPreProcessor<TRequest>
{
    public async Task PreProcessAsync(IPreProcessorContext<TRequest> context, CancellationToken cancellationToken)
    {
        var logger = context.HttpContext.Resolve<ILogger<TRequest>>();

        // Log the incoming request details
        Console.WriteLine($"Processing request for endpoint: {context.HttpContext.Request.Path}");
        
        logger.LogInformation($"Processing request for endpoint: {context.HttpContext.Request.Path}");
        
        await Task.CompletedTask; // Simulate async operation
    }
}
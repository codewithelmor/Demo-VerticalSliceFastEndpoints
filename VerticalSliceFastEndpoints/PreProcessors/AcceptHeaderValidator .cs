namespace VerticalSliceFastEndpoints.PreProcessors
{
    public class AcceptHeaderValidator : IGlobalPreProcessor
    {
        private static readonly string[] Supported = { MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml, MediaTypeNames.Text.Xml };

        public Task PreProcessAsync(IPreProcessorContext ctx, CancellationToken ct)
        {
            var accept = ctx.HttpContext.Request.Headers.Accept.ToString();

            var isAcceptable =
                string.IsNullOrEmpty(accept) ||
                accept.Contains("*/*") ||
                Supported.Any(s => accept.Contains(s, StringComparison.OrdinalIgnoreCase));

            if (!isAcceptable && !ctx.HttpContext.ResponseStarted())
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                return ctx.HttpContext.Response.StartAsync(ct);
            }

            return Task.CompletedTask;
        }
    }
}

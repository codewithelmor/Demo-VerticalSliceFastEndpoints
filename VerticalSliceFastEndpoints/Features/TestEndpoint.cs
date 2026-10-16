namespace VerticalSliceFastEndpoints.Features
{
    public class TestEndpoint : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/test");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            await Send.OkAsync(new
            {
                Message = "FastEndpoints is working"
            }, cancellation: ct);
        }
    }
}

namespace VerticalSliceFastEndpoints.Infrastructure
{
    public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
    {
        public string? UserId => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}

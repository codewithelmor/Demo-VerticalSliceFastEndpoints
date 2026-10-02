namespace VerticalSliceFastEndpoints.Core.Interfaces
{
    public interface IJwtTokenService
    {
        string CreateToken(ApplicationUser user, IEnumerable<string> roles);
    }
}

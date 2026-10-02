namespace VerticalSliceFastEndpoints.Options
{
    public class JwtSettings
    {
        public const string SectionName = "ApiSettings";

        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 60;
    }
}

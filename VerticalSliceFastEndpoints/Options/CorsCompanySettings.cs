namespace VerticalSliceFastEndpoints.Options
{
    public class CorsCompanySettings
    {
        public const string SectionName = "Cors";

        public string Company { get; set; } = string.Empty;
        public string[] AllowedOrigins { get; set; } = [];
        public string[] AllowedMethods { get; set; } = [];
        public string[] AllowedHeaders { get; set; } = [];
    }
}

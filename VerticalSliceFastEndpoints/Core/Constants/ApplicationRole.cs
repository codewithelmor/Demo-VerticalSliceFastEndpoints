namespace VerticalSliceFastEndpoints.Core.Constants
{
    public static class ApplicationRole
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";

        // This property extracts all constant values into an array
        public static readonly string[] AllRoles = typeof(ApplicationRole)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue())
            .ToArray();
    }
}

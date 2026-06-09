namespace LightMES.Domain.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
        public const string ToggleStatus = "Permissions.Users.ToggleStatus";
        public const string ChangePassword = "Permissions.Users.ChangePassword";
    }
    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Manage = "Permissions.Roles.Manage";
    }
    public static class Production
    {
        public const string StartOrder = "Permissions.Production.StartOrder";
        public const string SkipProcess = "Permissions.Production.SkipProcess";
        public const string ScrapMaterial = "Permissions.Production.ScrapMaterial";
    }
    private static readonly HashSet<string> _allPermissions;
    static Permissions()
    {
        _allPermissions = typeof(Permissions)
            .GetNestedTypes()
            .SelectMany(t => t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy))
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToHashSet();
    }
    public static bool IsValidPermission(string permission) => _allPermissions.Contains(permission);
    public static IReadOnlyCollection<string> GetAll() => _allPermissions;
}

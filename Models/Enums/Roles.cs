namespace DairyManagementSystem.Models.Enums
{
    // Plain string constants, not a C# enum — Identity's role system (AspNetRoles)
    // is fundamentally string/name-based (IdentityRole.Name), and [Authorize(Roles="...")]
    // takes strings too. A C# enum would just force us to .ToString() everywhere;
    // constants give us compile-time-checked "no magic strings" without that friction.
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Operator = "Operator";
        public const string Farmer = "Farmer";

        public static readonly string[] All = { Admin, Operator, Farmer };
    }
}

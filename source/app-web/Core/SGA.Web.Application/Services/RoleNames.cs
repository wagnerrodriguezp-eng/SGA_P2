namespace SGA.Web.Application.Services;

// Must stay in sync with SGA.Identity.Constants.RoleNames — Web.Application does not reference
// SGA.Identity directly (role assignment flows through IIdentityGateway using plain strings).
internal static class RoleNames
{
    public const string Student = "Student";
    public const string Employee = "Employee";
    public const string Driver = "Driver";
}

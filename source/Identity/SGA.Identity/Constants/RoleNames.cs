namespace SGA.Identity.Constants;

public static class RoleNames
{
    public const string TransportAdmin = "TransportAdmin";
    public const string Student = "Student";
    public const string Employee = "Employee";
    public const string Driver = "Driver";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        TransportAdmin, Student, Employee, Driver
    };
}

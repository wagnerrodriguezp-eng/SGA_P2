using System.ComponentModel.DataAnnotations;

namespace SGA.Desktop.Domain.Dtos;

public enum RegistrationRole
{
    Student = 1,
    Employee = 2,
    Driver = 3
}

// Administrative provisioning shape used by POST /api/admin/users — the account still only ever
// authenticates on app-web-mvc, per the application-affinity rule.
public class RegisterAccountRequestDto
{
    [Required, MaxLength(256), EmailAddress]
    public string Email { get; set; } = string.Empty;

    // Optional — the administrator does not set the user's password here. If omitted, the service
    // generates a random, unusable initial password; the user later confirms their email and sets
    // their own password via app-web-mvc's existing "forgot password" flow.
    public string? Password { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public RegistrationRole Role { get; set; }

    [MaxLength(20)]
    public string? StudentCode { get; set; }

    [MaxLength(20)]
    public string? EmployeeCode { get; set; }

    [MaxLength(20)]
    public string? LicenseNumber { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace SGA.Web.Application.Dtos;

public enum RegistrationRole
{
    Student = 1,
    Employee = 2,
    Driver = 3
}

public class RegisterAccountRequestDto
{
    [Required, MaxLength(256), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public RegistrationRole Role { get; set; }

    // Only one of these is required depending on Role — validated in the Application service,
    // not via data annotations, since the requirement is conditional on another field.
    [MaxLength(20)]
    public string? StudentCode { get; set; }

    [MaxLength(20)]
    public string? EmployeeCode { get; set; }

    [MaxLength(20)]
    public string? LicenseNumber { get; set; }
}

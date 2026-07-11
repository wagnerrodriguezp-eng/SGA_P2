using System.ComponentModel.DataAnnotations;

namespace SGA.Desktop.Domain.Dtos;

public class CreateDriverDto
{
    [Required, MaxLength(20)]
    public string LicenseNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required, MaxLength(256), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
}

public class UpdateDriverDto
{
    [Required, MaxLength(20)]
    public string LicenseNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}

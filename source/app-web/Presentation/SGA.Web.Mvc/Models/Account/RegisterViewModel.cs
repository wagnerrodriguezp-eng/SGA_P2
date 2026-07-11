using System.ComponentModel.DataAnnotations;

namespace SGA.Web.Mvc.Models.Account;

public enum RegistrationRole
{
    Student = 1,
    Employee = 2,
    Driver = 3
}

public class RegisterViewModel
{
    [Required, EmailAddress, MaxLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public RegistrationRole Role { get; set; }

    [MaxLength(20)]
    [Display(Name = "Student code")]
    public string? StudentCode { get; set; }

    [MaxLength(20)]
    [Display(Name = "Employee code")]
    public string? EmployeeCode { get; set; }

    [MaxLength(20)]
    [Display(Name = "License number")]
    public string? LicenseNumber { get; set; }
}

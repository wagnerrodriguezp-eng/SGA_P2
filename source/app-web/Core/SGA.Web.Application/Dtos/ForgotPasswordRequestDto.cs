using System.ComponentModel.DataAnnotations;

namespace SGA.Web.Application.Dtos;

public class ForgotPasswordRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

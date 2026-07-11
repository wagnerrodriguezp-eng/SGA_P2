using System.ComponentModel.DataAnnotations;

namespace SGA.Web.Application.Dtos;

public class ResetPasswordRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}

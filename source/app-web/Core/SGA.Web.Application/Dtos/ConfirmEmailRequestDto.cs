using System.ComponentModel.DataAnnotations;

namespace SGA.Web.Application.Dtos;

public class ConfirmEmailRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;
}

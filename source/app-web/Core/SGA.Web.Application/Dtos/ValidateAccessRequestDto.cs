using System.ComponentModel.DataAnnotations;

namespace SGA.Web.Application.Dtos;

public class ValidateAccessRequestDto
{
    [Required, MaxLength(20)]
    public string AuthorizationIdentifier { get; set; } = string.Empty;

    [Required]
    public Guid TripId { get; set; }
}

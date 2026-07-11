using System.ComponentModel.DataAnnotations;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.Desktop.Domain.Dtos;

public class CreateAuthorizationDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public AuthorizationType AuthorizationType { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? Balance { get; set; }
}

public class UpdateAuthorizationDto
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? Balance { get; set; }

    [Required]
    public AuthorizationStatus AuthorizationStatus { get; set; }
}

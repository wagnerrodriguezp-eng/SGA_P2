using System.ComponentModel.DataAnnotations;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.Web.Application.Dtos;

public class ReportIncidentRequestDto
{
    [Required]
    public IncidentType IncidentType { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}

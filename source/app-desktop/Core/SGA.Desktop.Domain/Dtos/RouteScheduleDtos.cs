using System.ComponentModel.DataAnnotations;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.Desktop.Domain.Dtos;

public class CreateRouteDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdateRouteDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public RouteStatus RouteStatus { get; set; }
}

public class CreateStopDto
{
    [Required]
    public Guid RouteId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Order { get; set; }
}

public class CreateScheduleDto
{
    [Required]
    public Guid RouteId { get; set; }

    [Required]
    public TimeSpan DepartureTime { get; set; }

    [Required]
    public int DaysOfWeekMask { get; set; }
}

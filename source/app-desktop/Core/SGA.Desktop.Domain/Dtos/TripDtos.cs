using System.ComponentModel.DataAnnotations;

namespace SGA.Desktop.Domain.Dtos;

public class AssignTripDto
{
    [Required]
    public Guid ScheduleId { get; set; }

    [Required]
    public Guid BusId { get; set; }

    [Required]
    public Guid DriverUserId { get; set; }

    [Required]
    public DateOnly TripDate { get; set; }
}

public class CancelTripDto
{
    [Required]
    public Guid TripId { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

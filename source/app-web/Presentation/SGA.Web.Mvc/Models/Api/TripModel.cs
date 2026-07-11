namespace SGA.Web.Mvc.Models.Api;

public class TripModel
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid BusId { get; set; }
    public DateOnly TripDate { get; set; }
    public int TripStatus { get; set; }
    public int MaxCapacitySnapshot { get; set; }
    public int CapacityUsed { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
}

public class IncidentModel
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public int IncidentType { get; set; }
    public string Description { get; set; } = string.Empty;
    public int IncidentStatus { get; set; }
}

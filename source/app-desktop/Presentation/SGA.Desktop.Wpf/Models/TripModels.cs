namespace SGA.Desktop.Wpf.Models;

public enum TripStatusOption
{
    Scheduled = 1,
    InProgress = 2,
    Delayed = 3,
    Completed = 4,
    Cancelled = 5
}

public class TripModel
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid BusId { get; set; }
    public Guid DriverUserId { get; set; }
    public DateOnly TripDate { get; set; }
    public TripStatusOption TripStatus { get; set; }
    public int MaxCapacitySnapshot { get; set; }
    public int CapacityUsed { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
}

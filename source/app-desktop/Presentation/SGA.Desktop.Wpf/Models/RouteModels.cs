namespace SGA.Desktop.Wpf.Models;

public enum RouteStatusOption
{
    Active = 1,
    Inactive = 2
}

public class RouteModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RouteStatusOption RouteStatus { get; set; }
}

public class StopModel
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class ScheduleModel
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public TimeSpan DepartureTime { get; set; }
    public int DaysOfWeekMask { get; set; }
}

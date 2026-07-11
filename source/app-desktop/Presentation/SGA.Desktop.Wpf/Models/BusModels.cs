namespace SGA.Desktop.Wpf.Models;

public enum BusStatusOption
{
    Active = 1,
    InMaintenance = 2,
    OutOfService = 3,
    Retired = 4
}

public class BusModel
{
    public Guid Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string? Model { get; set; }
    public int? Year { get; set; }
    public int Capacity { get; set; }
    public BusStatusOption BusStatus { get; set; }
}

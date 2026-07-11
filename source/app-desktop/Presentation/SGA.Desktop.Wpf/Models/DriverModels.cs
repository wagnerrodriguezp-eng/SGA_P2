namespace SGA.Desktop.Wpf.Models;

public class DriverProfileModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

namespace SGA.Desktop.Wpf.Models;

public enum AuthorizationTypeOption
{
    MonthlyTicket = 1,
    RechargeableCard = 2
}

public enum AuthorizationStatusOption
{
    Active = 1,
    Expired = 2,
    Suspended = 3,
    Cancelled = 4
}

public class AuthorizationModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AuthorizationTypeOption AuthorizationType { get; set; }
    public AuthorizationStatusOption AuthorizationStatus { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? Balance { get; set; }
    public DateTime IssuedAtUtc { get; set; }
}

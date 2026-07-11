namespace SGA.Web.Mvc.Models.Api;

public enum AccessResultCode
{
    Granted = 1,
    DeniedExpired = 2,
    DeniedNoBalance = 3,
    DeniedNoCapacity = 4,
    DeniedUnauthorized = 5
}

public class UsageRecordModel
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public DateTime UsedAtUtc { get; set; }
    public AccessResultCode AccessResult { get; set; }
}

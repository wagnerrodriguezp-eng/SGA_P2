namespace SGA.Desktop.Wpf.Models;

public class AuditLogModel
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? Details { get; set; }
}

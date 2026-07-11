using SGA.SharedKernel.Domain.Common;

namespace SGA.SharedKernel.Domain.Entities;

public class AuditLog : BaseEntity<Guid>
{
    // Null for system/anonymous actions (e.g., a rejected login before identity is known).
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;

    // Stored as string to accommodate any key shape.
    public string? EntityId { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}

using SGA.SharedKernel.Domain.Common;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.SharedKernel.Domain.Entities;

public class UsageRecord : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public Guid TripId { get; set; }

    // Null when AccessResult is DeniedUnauthorized (no authorization found at all).
    public Guid? AuthorizationId { get; set; }
    public DateTime UsedAtUtc { get; set; }
    public AccessResult AccessResult { get; set; }
}

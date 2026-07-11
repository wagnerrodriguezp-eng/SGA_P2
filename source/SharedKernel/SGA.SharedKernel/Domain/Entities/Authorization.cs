using SGA.SharedKernel.Domain.Common;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.SharedKernel.Domain.Entities;

// Single table for both authorization kinds (no inheritance), discriminated by AuthorizationType.
public class Authorization : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public AuthorizationType AuthorizationType { get; set; }
    public AuthorizationStatus AuthorizationStatus { get; set; }

    // Used by MonthlyTicket; still populated (as issue date) for RechargeableCard.
    public DateOnly StartDate { get; set; }

    // Used by MonthlyTicket only — vigency end date.
    public DateOnly? EndDate { get; set; }

    // Used by RechargeableCard only — remaining trip count.
    public int? Balance { get; set; }
    public DateTime IssuedAtUtc { get; set; }
}

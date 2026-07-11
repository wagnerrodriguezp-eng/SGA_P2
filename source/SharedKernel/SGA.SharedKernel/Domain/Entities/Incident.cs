using SGA.SharedKernel.Domain.Common;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.SharedKernel.Domain.Entities;

public class Incident : BaseEntity<Guid>
{
    public Guid TripId { get; set; }
    public Guid ReportedByUserId { get; set; }
    public IncidentType IncidentType { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ReportedAtUtc { get; set; }
    public IncidentStatus IncidentStatus { get; set; }
}

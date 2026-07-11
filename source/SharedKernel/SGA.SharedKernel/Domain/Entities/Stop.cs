using SGA.SharedKernel.Domain.Common;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.SharedKernel.Domain.Entities;

public class Stop : BaseEntity<Guid>
{
    public Guid RouteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public StopStatus StopStatus { get; set; }
}

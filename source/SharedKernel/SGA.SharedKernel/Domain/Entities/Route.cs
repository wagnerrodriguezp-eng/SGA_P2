using SGA.SharedKernel.Domain.Common;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.SharedKernel.Domain.Entities;

public class Route : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RouteStatus RouteStatus { get; set; }
}

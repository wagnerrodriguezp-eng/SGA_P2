using SGA.SharedKernel.Domain.Common;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.SharedKernel.Domain.Entities;

public class Bus : BaseEntity<Guid>
{
    public string PlateNumber { get; set; } = string.Empty;
    public string? Model { get; set; }
    public int? Year { get; set; }
    public int Capacity { get; set; }
    public BusStatus BusStatus { get; set; }
}

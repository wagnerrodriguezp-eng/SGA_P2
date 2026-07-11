using SGA.SharedKernel.Domain.Common;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.SharedKernel.Domain.Entities;

public class Trip : BaseEntity<Guid>
{
    public Guid ScheduleId { get; set; }
    public Guid BusId { get; set; }
    public Guid DriverUserId { get; set; }
    public DateOnly TripDate { get; set; }
    public TripStatus TripStatus { get; set; }

    // Copied from Bus.Capacity at trip-assignment time so later bus capacity changes never
    // retroactively affect an in-progress trip's accounting.
    public int MaxCapacitySnapshot { get; set; }
    public int CapacityUsed { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }

    // EF Core concurrency token backing the optimistic-concurrency capacity control.
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

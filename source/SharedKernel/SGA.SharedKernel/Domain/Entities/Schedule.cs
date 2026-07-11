using SGA.SharedKernel.Domain.Common;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.SharedKernel.Domain.Entities;

public class Schedule : BaseEntity<Guid>
{
    public Guid RouteId { get; set; }
    public TimeSpan DepartureTime { get; set; }

    // Bit mask: Mon=1, Tue=2, Wed=4, Thu=8, Fri=16, Sat=32, Sun=64
    public int DaysOfWeekMask { get; set; }
    public ScheduleStatus ScheduleStatus { get; set; }
}

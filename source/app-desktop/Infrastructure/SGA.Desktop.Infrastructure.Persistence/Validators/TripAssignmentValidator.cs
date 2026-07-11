using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Enums;
using SGA.SharedKernel.Domain.Notifications;
using SGA.Desktop.Application.Persistence;
using SGA.Desktop.Domain.Validation;

namespace SGA.Desktop.Infrastructure.Persistence.Validators;

public class TripAssignmentValidator : ITripAssignmentValidator
{
    private readonly DesktopAppDbContext _context;
    private readonly ITripRepository _tripRepository;

    public TripAssignmentValidator(DesktopAppDbContext context, ITripRepository tripRepository)
    {
        _context = context;
        _tripRepository = tripRepository;
    }

    public async Task<NotificationContext> ValidateAssignmentAsync(
        Guid scheduleId, Guid busId, Guid driverUserId, DateOnly tripDate, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();

        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == scheduleId, ct);
        if (schedule is null || schedule.ScheduleStatus != ScheduleStatus.Active)
        {
            notifications.AddNotification("ScheduleId", "Schedule not found or inactive.");
            return notifications;
        }

        var bus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == busId, ct);
        if (bus is null || bus.BusStatus != BusStatus.Active)
        {
            notifications.AddNotification("BusId", "Bus not found or not currently active.");
            return notifications;
        }

        if (!await _context.DriverProfiles.AnyAsync(d => d.UserId == driverUserId, ct))
        {
            notifications.AddNotification("DriverUserId", "Driver not found.");
            return notifications;
        }

        var overlapping = await _tripRepository.HasOverlappingAssignmentAsync(
            busId, driverUserId, tripDate, schedule.DepartureTime, ct);
        if (overlapping)
        {
            notifications.AddNotification(
                "Overlap", "The bus or driver is already assigned to another trip within an hour of this departure time.");
        }

        return notifications;
    }
}

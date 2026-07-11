using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Notifications;
using SGA.Desktop.Domain.Dtos;
using SGA.Desktop.Domain.Validation;

namespace SGA.Desktop.Infrastructure.Persistence.Validators;

public class RouteScheduleValidator : IRouteScheduleValidator
{
    private readonly DesktopAppDbContext _context;

    public RouteScheduleValidator(DesktopAppDbContext context)
    {
        _context = context;
    }

    public Task<NotificationContext> ValidateRouteForCreateAsync(CreateRouteDto dto, CancellationToken ct = default) =>
        Task.FromResult(new NotificationContext());

    public async Task<NotificationContext> ValidateRouteForUpdateAsync(Guid routeId, UpdateRouteDto dto, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        if (!await _context.Routes.AnyAsync(r => r.Id == routeId, ct))
        {
            notifications.AddNotification("NotFound", "Route not found.");
        }
        return notifications;
    }

    public async Task<NotificationContext> ValidateStopForCreateAsync(CreateStopDto dto, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        if (!await _context.Routes.AnyAsync(r => r.Id == dto.RouteId, ct))
        {
            notifications.AddNotification("RouteId", "Route not found.");
            return notifications;
        }

        if (await _context.Stops.AnyAsync(s => s.RouteId == dto.RouteId && s.Order == dto.Order, ct))
        {
            notifications.AddNotification("Order", "A stop with this order already exists on this route.");
        }

        return notifications;
    }

    public async Task<NotificationContext> ValidateScheduleForCreateAsync(CreateScheduleDto dto, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        if (!await _context.Routes.AnyAsync(r => r.Id == dto.RouteId, ct))
        {
            notifications.AddNotification("RouteId", "Route not found.");
            return notifications;
        }

        var routeSchedules = await _context.Schedules.Where(s => s.RouteId == dto.RouteId).ToListAsync(ct);
        var overlapping = routeSchedules.Any(s =>
            (s.DaysOfWeekMask & dto.DaysOfWeekMask) != 0 && s.DepartureTime == dto.DepartureTime);
        if (overlapping)
        {
            notifications.AddNotification(
                "DepartureTime", "A schedule with this departure time already exists on an overlapping day for this route.");
        }

        return notifications;
    }
}

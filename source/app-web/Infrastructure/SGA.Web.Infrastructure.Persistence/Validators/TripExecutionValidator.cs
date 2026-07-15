using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Enums;
using SGA.SharedKernel.Domain.Notifications;
using SGA.Web.Domain.Validation;

namespace SGA.Web.Infrastructure.Persistence.Validators;

public class TripExecutionValidator : ITripExecutionValidator
{
    private readonly WebAppDbContext _context;

    public TripExecutionValidator(WebAppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationContext> ValidateStartAsync(Guid tripId, Guid driverUserId, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct);

        if (trip is null)
        {
            notifications.AddNotification("NotFound", "Trip not found.");
            return notifications;
        }
        if (trip.DriverUserId != driverUserId)
        {
            notifications.AddNotification("Forbidden", "This trip is not assigned to you.");
            return notifications;
        }
        if (trip.TripStatus != TripStatus.Scheduled)
        {
            notifications.AddNotification("InvalidTransition", "This trip cannot be started from its current state.");
        }

        return notifications;
    }

    public async Task<NotificationContext> ValidateEndAsync(Guid tripId, Guid driverUserId, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct);

        if (trip is null)
        {
            notifications.AddNotification("NotFound", "Trip not found.");
            return notifications;
        }
        if (trip.DriverUserId != driverUserId)
        {
            notifications.AddNotification("Forbidden", "This trip is not assigned to you.");
            return notifications;
        }
        if (trip.TripStatus != TripStatus.InProgress)
        {
            notifications.AddNotification("InvalidTransition", "This trip cannot be ended from its current state.");
        }

        return notifications;
    }

    public async Task<NotificationContext> ValidateIncidentReportAsync(Guid tripId, Guid reportedByUserId, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct);

        if (trip is null)
        {
            notifications.AddNotification("NotFound", "Trip not found.");
            return notifications;
        }
        if (trip.DriverUserId != reportedByUserId)
        {
            notifications.AddNotification("Forbidden", "This trip is not assigned to you.");
        }

        return notifications;
    }

    public async Task<NotificationContext> ValidateIncidentsQueryAsync(Guid tripId, Guid driverUserId, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct);

        if (trip is null)
        {
            notifications.AddNotification("NotFound", "Trip not found.");
            return notifications;
        }
        if (trip.DriverUserId != driverUserId)
        {
            notifications.AddNotification("Forbidden", "This trip is not assigned to you.");
        }

        return notifications;
    }
}

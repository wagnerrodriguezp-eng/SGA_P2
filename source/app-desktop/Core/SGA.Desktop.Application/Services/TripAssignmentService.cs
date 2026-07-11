using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Desktop.Application.Auditing;
using SGA.Desktop.Application.Identity;
using SGA.Desktop.Application.Notifications;
using SGA.Desktop.Application.Persistence;
using SGA.Desktop.Domain.Dtos;
using SGA.Desktop.Domain.Validation;

namespace SGA.Desktop.Application.Services;

public class TripAssignmentService
{
    private readonly ITripAssignmentValidator _validator;
    private readonly ITripRepository _tripRepository;
    private readonly IGenericRepository<Bus, Guid> _buses;
    private readonly IGenericRepository<UsageRecord, Guid> _usageRecords;
    private readonly IIdentityGateway _identityGateway;
    private readonly INotificationSender _notificationSender;
    private readonly IAuditWriter _auditWriter;

    public TripAssignmentService(
        ITripAssignmentValidator validator,
        ITripRepository tripRepository,
        IGenericRepository<Bus, Guid> buses,
        IGenericRepository<UsageRecord, Guid> usageRecords,
        IIdentityGateway identityGateway,
        INotificationSender notificationSender,
        IAuditWriter auditWriter)
    {
        _validator = validator;
        _tripRepository = tripRepository;
        _buses = buses;
        _usageRecords = usageRecords;
        _identityGateway = identityGateway;
        _notificationSender = notificationSender;
        _auditWriter = auditWriter;
    }

    public Task<IReadOnlyList<Trip>> GetAllForDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default) =>
        _tripRepository.GetAllForDateRangeAsync(from, to, ct);

    public async Task<OperationResult<Trip>> AssignAsync(AssignTripDto dto, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateAssignmentAsync(dto.ScheduleId, dto.BusId, dto.DriverUserId, dto.TripDate, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Trip>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var bus = await _buses.GetByIdAsync(dto.BusId, ct);
        if (bus is null)
        {
            return OperationResult<Trip>.Failure(OperationResultStatus.NotFound, "Bus not found.");
        }

        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            ScheduleId = dto.ScheduleId,
            BusId = dto.BusId,
            DriverUserId = dto.DriverUserId,
            TripDate = dto.TripDate,
            TripStatus = TripStatus.Scheduled,
            MaxCapacitySnapshot = bus.Capacity,
            CapacityUsed = 0,
            RowVersion = Array.Empty<byte>()
        };
        await _tripRepository.AddAsync(trip, ct);
        await _tripRepository.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Trip.Assign", nameof(Trip), trip.Id.ToString(), null, null, ct);
        return OperationResult<Trip>.Success(trip);
    }

    public async Task<OperationResult<Trip>> CancelAsync(CancelTripDto dto, CancellationToken ct = default)
    {
        var trip = await _tripRepository.GetByIdAsync(dto.TripId, ct);
        if (trip is null)
        {
            return OperationResult<Trip>.Failure(OperationResultStatus.NotFound, "Trip not found.");
        }
        if (trip.TripStatus is TripStatus.Completed or TripStatus.Cancelled)
        {
            return OperationResult<Trip>.Failure(
                OperationResultStatus.Conflict, "This trip cannot be cancelled from its current state.");
        }

        trip.TripStatus = TripStatus.Cancelled;
        await _tripRepository.UpdateAsync(trip, ct);
        await _tripRepository.SaveChangesAsync(ct);

        await NotifyAffectedUsersAsync(trip, dto.Reason, ct);

        await _auditWriter.WriteAsync("Trip.Cancel", nameof(Trip), trip.Id.ToString(), null, dto.Reason, ct);
        return OperationResult<Trip>.Success(trip);
    }

    // The one true "fan-out" case in this system — every UsageRecord holder plus the assigned
    // driver gets their own independent NotificationMessage, so one delivery failure never blocks
    // or rolls back notifying anyone else. See app-desktop-wpf/07-messaging-integration.md §2.
    private async Task NotifyAffectedUsersAsync(Trip trip, string reason, CancellationToken ct)
    {
        var usageRecords = await _usageRecords.GetAllAsync(ct: ct);
        var affectedUserIds = usageRecords
            .Where(u => u.TripId == trip.Id && u.AccessResult == AccessResult.Granted)
            .Select(u => u.UserId)
            .Append(trip.DriverUserId)
            .Distinct()
            .ToList();

        var emails = await _identityGateway.GetUserEmailsAsync(affectedUserIds, ct);
        var tripDescription = $"Trip on {trip.TripDate:yyyy-MM-dd}";

        foreach (var email in emails.Values)
        {
            await _notificationSender.SendTripCancellationAsync(email, tripDescription, reason, ct);
        }
    }
}

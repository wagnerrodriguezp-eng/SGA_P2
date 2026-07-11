using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Web.Application.Auditing;
using SGA.Web.Application.Dtos;
using SGA.Web.Application.Notifications;
using SGA.Web.Application.Persistence;
using SGA.Web.Domain.Validation;

namespace SGA.Web.Application.Services;

public class TripExecutionService
{
    private readonly ITripExecutionValidator _validator;
    private readonly ITripRepository _tripRepository;
    private readonly IGenericRepository<Incident, Guid> _incidentRepository;
    private readonly IAuditWriter _auditWriter;
    private readonly INotificationSender _notificationSender;

    public TripExecutionService(
        ITripExecutionValidator validator,
        ITripRepository tripRepository,
        IGenericRepository<Incident, Guid> incidentRepository,
        IAuditWriter auditWriter,
        INotificationSender notificationSender)
    {
        _validator = validator;
        _tripRepository = tripRepository;
        _incidentRepository = incidentRepository;
        _auditWriter = auditWriter;
        _notificationSender = notificationSender;
    }

    public Task<IReadOnlyList<Trip>> GetAssignedTodayAsync(Guid driverUserId, CancellationToken ct = default) =>
        _tripRepository.GetAssignedToDriverTodayAsync(driverUserId, ct);

    public async Task<OperationResult<Trip>> StartTripAsync(Guid tripId, Guid driverUserId, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateStartAsync(tripId, driverUserId, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Trip>.Failure(
                OperationResultStatus.Conflict, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var trip = (await _tripRepository.GetByIdAsync(tripId, ct))!;
        trip.TripStatus = TripStatus.InProgress;
        trip.StartedAtUtc = DateTime.UtcNow;
        await _tripRepository.UpdateAsync(trip, ct);
        await _tripRepository.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Trip.Start", nameof(Trip), tripId.ToString(), driverUserId, null, ct);
        return OperationResult<Trip>.Success(trip);
    }

    public async Task<OperationResult<Trip>> EndTripAsync(Guid tripId, Guid driverUserId, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateEndAsync(tripId, driverUserId, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Trip>.Failure(
                OperationResultStatus.Conflict, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var trip = (await _tripRepository.GetByIdAsync(tripId, ct))!;
        trip.TripStatus = TripStatus.Completed;
        trip.EndedAtUtc = DateTime.UtcNow;
        await _tripRepository.UpdateAsync(trip, ct);
        await _tripRepository.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Trip.End", nameof(Trip), tripId.ToString(), driverUserId, null, ct);
        return OperationResult<Trip>.Success(trip);
    }

    public async Task<OperationResult<Incident>> ReportIncidentAsync(
        Guid tripId, Guid reportedByUserId, ReportIncidentRequestDto dto, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateIncidentReportAsync(tripId, reportedByUserId, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Incident>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            ReportedByUserId = reportedByUserId,
            IncidentType = dto.IncidentType,
            Description = dto.Description,
            ReportedAtUtc = DateTime.UtcNow,
            IncidentStatus = IncidentStatus.Reported
        };
        await _incidentRepository.AddAsync(incident, ct);
        await _incidentRepository.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync(
            "Incident.Report", nameof(Incident), incident.Id.ToString(), reportedByUserId, dto.Description, ct);

        // Notifies the Transport Administrator distribution address, never passengers directly —
        // passenger notification only happens if the Administrator escalates to a cancellation
        // from app-desktop-wpf.
        var trip = await _tripRepository.GetByIdAsync(tripId, ct);
        var tripDescription = trip is null ? tripId.ToString() : $"Trip {trip.Id} on {trip.TripDate}";
        await _notificationSender.SendIncidentNoticeAsync(tripDescription, dto.Description, ct);

        return OperationResult<Incident>.Success(incident);
    }
}

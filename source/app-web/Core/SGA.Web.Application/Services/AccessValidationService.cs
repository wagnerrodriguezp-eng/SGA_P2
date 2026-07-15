using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.SharedKernel.Domain.Notifications;
using SGA.Web.Application.Auditing;
using SGA.Web.Application.Persistence;
using SGA.Web.Domain.Validation;

namespace SGA.Web.Application.Services;

// Orchestrates the boarding flow described in shared-kernel/05-business-rules-to-domain-mapping.md
// §1. Never contains business logic itself — the three-condition check lives in IAccessValidator.
public class AccessValidationService
{
    private const int MaxConcurrencyRetries = 3;

    private readonly IAccessValidator _accessValidator;
    private readonly ITripRepository _tripRepository;
    private readonly IAuthorizationRepository _authorizationRepository;
    private readonly IGenericRepository<UsageRecord, Guid> _usageRecordRepository;
    private readonly IAuditWriter _auditWriter;

    public AccessValidationService(
        IAccessValidator accessValidator,
        ITripRepository tripRepository,
        IAuthorizationRepository authorizationRepository,
        IGenericRepository<UsageRecord, Guid> usageRecordRepository,
        IAuditWriter auditWriter)
    {
        _accessValidator = accessValidator;
        _tripRepository = tripRepository;
        _authorizationRepository = authorizationRepository;
        _usageRecordRepository = usageRecordRepository;
        _auditWriter = auditWriter;
    }

    public async Task<OperationResult<UsageRecord>> ValidateAndRecordAsync(
        Guid userId, Guid tripId, CancellationToken ct = default)
    {
        for (var attempt = 1; attempt <= MaxConcurrencyRetries; attempt++)
        {
            var notifications = await _accessValidator.ValidateAccessAsync(userId, tripId, ct);

            if (notifications.HasNotifications)
            {
                var deniedResult = MapToAccessResult(notifications);
                var deniedRecord = await RecordUsageAsync(userId, tripId, null, deniedResult, ct);
                await _auditWriter.WriteAsync(
                    "Access.Denied", nameof(Trip), tripId.ToString(), userId,
                    string.Join("; ", notifications.Notifications.Select(n => n.Message)), ct);
                return OperationResult<UsageRecord>.Success(deniedRecord);
            }

            try
            {
                var authorization = await _authorizationRepository.GetActiveForUserAsync(userId, ct)
                    ?? throw new InvalidOperationException("Validator passed but authorization was not found.");

                if (authorization.AuthorizationType == AuthorizationType.RechargeableCard)
                {
                    authorization.Balance = (authorization.Balance ?? 0) - 1;
                    await _authorizationRepository.UpdateAsync(authorization, ct);
                }

                var trip = await _tripRepository.GetWithConcurrencyTokenAsync(tripId, ct)
                    ?? throw new InvalidOperationException("Validator passed but trip was not found.");
                trip.CapacityUsed += 1;
                await _tripRepository.UpdateAsync(trip, ct);

                var grantedRecord = new UsageRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TripId = tripId,
                    AuthorizationId = authorization.Id,
                    UsedAtUtc = DateTime.UtcNow,
                    AccessResult = AccessResult.Granted
                };
                await _usageRecordRepository.AddAsync(grantedRecord, ct);

                // One SaveChangesAsync commits the trip/authorization/usage-record change set
                // together; EF Core's EnableRetryOnFailure covers transient infra faults here
                // transparently — a RowVersion mismatch surfaces as ConcurrencyConflictException.
                await _tripRepository.SaveChangesAsync(ct);

                await _auditWriter.WriteAsync("Access.Granted", nameof(Trip), tripId.ToString(), userId, null, ct);
                return OperationResult<UsageRecord>.Success(grantedRecord);
            }
            catch (ConcurrencyConflictException) when (attempt < MaxConcurrencyRetries)
            {
                // Another request consumed the last seat first — loop again and re-validate fresh.
            }
            catch (ConcurrencyConflictException)
            {
                // The failed SaveChangesAsync above left the Trip/Authorization entries tracked as
                // Modified with a stale RowVersion. Reload them (same mechanism the retry path uses)
                // so the denial record below doesn't also try to re-save those stale changes and
                // throw a second, unhandled ConcurrencyConflictException.
                await _tripRepository.GetWithConcurrencyTokenAsync(tripId, ct);
                await _authorizationRepository.GetActiveForUserAsync(userId, ct);

                var deniedRecord = await RecordUsageAsync(userId, tripId, null, AccessResult.DeniedNoCapacity, ct);
                await _auditWriter.WriteAsync(
                    "Access.Denied", nameof(Trip), tripId.ToString(), userId,
                    "Trip reached full capacity after concurrent boarding attempts.", ct);
                return OperationResult<UsageRecord>.Success(deniedRecord);
            }
        }

        return OperationResult<UsageRecord>.Failure(
            OperationResultStatus.Conflict, "Unable to validate access right now, please try again.");
    }

    private async Task<UsageRecord> RecordUsageAsync(
        Guid userId, Guid tripId, Guid? authorizationId, AccessResult result, CancellationToken ct)
    {
        var record = new UsageRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TripId = tripId,
            AuthorizationId = authorizationId,
            UsedAtUtc = DateTime.UtcNow,
            AccessResult = result
        };
        await _usageRecordRepository.AddAsync(record, ct);
        await _usageRecordRepository.SaveChangesAsync(ct);
        return record;
    }

    // Keys written by AccessValidator (Infrastructure.Persistence) — see that implementation for
    // the exact three-condition check these map back to.
    private static AccessResult MapToAccessResult(NotificationContext notifications)
    {
        var key = notifications.Notifications.First().Key;
        return key switch
        {
            "Expired" => AccessResult.DeniedExpired,
            "NoBalance" => AccessResult.DeniedNoBalance,
            "NoCapacity" => AccessResult.DeniedNoCapacity,
            _ => AccessResult.DeniedUnauthorized
        };
    }
}

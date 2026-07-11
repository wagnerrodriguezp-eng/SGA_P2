using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Desktop.Application.Auditing;
using SGA.Desktop.Domain.Dtos;
using SGA.Desktop.Domain.Validation;

namespace SGA.Desktop.Application.Services;

public class AuthorizationManagementService
{
    private readonly IAuthorizationValidator _validator;
    private readonly IGenericRepository<Authorization, Guid> _authorizations;
    private readonly IAuditWriter _auditWriter;

    public AuthorizationManagementService(
        IAuthorizationValidator validator, IGenericRepository<Authorization, Guid> authorizations, IAuditWriter auditWriter)
    {
        _validator = validator;
        _authorizations = authorizations;
        _auditWriter = auditWriter;
    }

    public async Task<IReadOnlyList<Authorization>> GetForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var all = await _authorizations.GetAllAsync(ct: ct);
        return all.Where(a => a.UserId == userId).ToList();
    }

    public async Task<OperationResult<Authorization>> CreateAsync(CreateAuthorizationDto dto, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateForCreateAsync(dto, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Authorization>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var authorization = new Authorization
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            AuthorizationType = dto.AuthorizationType,
            AuthorizationStatus = AuthorizationStatus.Active,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Balance = dto.Balance,
            IssuedAtUtc = DateTime.UtcNow
        };
        await _authorizations.AddAsync(authorization, ct);
        await _authorizations.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Authorization.Issue", nameof(Authorization), authorization.Id.ToString(), null, null, ct);
        return OperationResult<Authorization>.Success(authorization);
    }

    public async Task<OperationResult<Authorization>> UpdateAsync(Guid id, UpdateAuthorizationDto dto, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateForUpdateAsync(id, dto, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Authorization>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var authorization = await _authorizations.GetByIdAsync(id, ct);
        if (authorization is null)
        {
            return OperationResult<Authorization>.Failure(OperationResultStatus.NotFound, "Authorization not found.");
        }

        if (dto.StartDate.HasValue) authorization.StartDate = dto.StartDate.Value;
        authorization.EndDate = dto.EndDate;
        authorization.Balance = dto.Balance;
        authorization.AuthorizationStatus = dto.AuthorizationStatus;
        await _authorizations.UpdateAsync(authorization, ct);
        await _authorizations.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Authorization.Update", nameof(Authorization), authorization.Id.ToString(), null, null, ct);
        return OperationResult<Authorization>.Success(authorization);
    }

    public async Task<OperationResult> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateForDeactivateAsync(id, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var authorization = await _authorizations.GetByIdAsync(id, ct);
        if (authorization is null)
        {
            return OperationResult.Failure(OperationResultStatus.NotFound, "Authorization not found.");
        }

        authorization.AuthorizationStatus = AuthorizationStatus.Cancelled;
        await _authorizations.UpdateAsync(authorization, ct);
        await _authorizations.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Authorization.Cancel", nameof(Authorization), authorization.Id.ToString(), null, null, ct);
        return OperationResult.Success("Authorization cancelled.");
    }
}

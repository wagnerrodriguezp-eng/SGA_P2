using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Auditing;
using SGA.Desktop.Application.Identity;
using SGA.Desktop.Application.Notifications;
using SGA.Desktop.Domain.Dtos;
using SGA.Desktop.Domain.Validation;

namespace SGA.Desktop.Application.Services;

public class DriverManagementService
{
    private readonly IDriverValidator _validator;
    private readonly IGenericRepository<DriverProfile, Guid> _driverProfiles;
    private readonly IIdentityGateway _identityGateway;
    private readonly INotificationSender _notificationSender;
    private readonly IAuditWriter _auditWriter;

    public DriverManagementService(
        IDriverValidator validator,
        IGenericRepository<DriverProfile, Guid> driverProfiles,
        IIdentityGateway identityGateway,
        INotificationSender notificationSender,
        IAuditWriter auditWriter)
    {
        _validator = validator;
        _driverProfiles = driverProfiles;
        _identityGateway = identityGateway;
        _notificationSender = notificationSender;
        _auditWriter = auditWriter;
    }

    public Task<IReadOnlyList<DriverProfile>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default) =>
        _driverProfiles.GetAllAsync(includeInactive, ct);

    public async Task<OperationResult<DriverProfile>> CreateAsync(
        CreateDriverDto dto, string confirmationLinkBase, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateForCreateAsync(dto, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<DriverProfile>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var registerResult = await _identityGateway.RegisterAccountAsync(
            dto.Email, GenerateTemporaryPassword(), dto.FirstName, dto.LastName, RoleNames.Driver, ct);
        if (!registerResult.IsSuccess || registerResult.Data is null)
        {
            return OperationResult<DriverProfile>.Failure(registerResult.Status, registerResult.Errors.ToArray());
        }

        var profile = new DriverProfile
        {
            Id = Guid.NewGuid(),
            UserId = registerResult.Data.UserId,
            LicenseNumber = dto.LicenseNumber,
            PhoneNumber = dto.PhoneNumber
        };
        await _driverProfiles.AddAsync(profile, ct);
        await _driverProfiles.SaveChangesAsync(ct);

        var confirmationLink = $"{confirmationLinkBase}?userId={profile.UserId}&token={Uri.EscapeDataString(registerResult.Data.EmailConfirmationToken)}";
        await _notificationSender.SendAccountConfirmationAsync(dto.Email, dto.FirstName, confirmationLink, ct);

        await _auditWriter.WriteAsync("Driver.Create", nameof(DriverProfile), profile.Id.ToString(), null, null, ct);
        return OperationResult<DriverProfile>.Success(profile);
    }

    public async Task<OperationResult<DriverProfile>> UpdateAsync(Guid id, UpdateDriverDto dto, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateForUpdateAsync(id, dto, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<DriverProfile>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var profile = await _driverProfiles.GetByIdAsync(id, ct);
        if (profile is null)
        {
            return OperationResult<DriverProfile>.Failure(OperationResultStatus.NotFound, "Driver not found.");
        }

        profile.LicenseNumber = dto.LicenseNumber;
        profile.PhoneNumber = dto.PhoneNumber;
        await _driverProfiles.UpdateAsync(profile, ct);
        await _driverProfiles.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Driver.Update", nameof(DriverProfile), profile.Id.ToString(), null, null, ct);
        return OperationResult<DriverProfile>.Success(profile);
    }

    public async Task<OperationResult> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateForDeactivateAsync(id, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        await _driverProfiles.DeactivateAsync(id, ct);
        await _driverProfiles.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Driver.Deactivate", nameof(DriverProfile), id.ToString(), null, null, ct);
        return OperationResult.Success("Driver deactivated.");
    }

    private static string GenerateTemporaryPassword() => $"Tmp!{Guid.NewGuid():N}9a";
}

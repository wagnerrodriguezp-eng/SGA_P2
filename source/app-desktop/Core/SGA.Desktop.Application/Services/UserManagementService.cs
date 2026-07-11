using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Auditing;
using SGA.Desktop.Application.Identity;
using SGA.Desktop.Application.Notifications;
using SGA.Desktop.Domain.Dtos;

namespace SGA.Desktop.Application.Services;

// Administrative provisioning for Student/Employee/Driver accounts — the credential still only
// ever works on app-web-mvc, per the application-affinity rule; this module just lets an
// administrator create it without requiring the person to self-register.
public class UserManagementService
{
    private readonly IIdentityGateway _identityGateway;
    private readonly INotificationSender _notificationSender;
    private readonly IGenericRepository<StudentProfile, Guid> _studentProfiles;
    private readonly IGenericRepository<EmployeeProfile, Guid> _employeeProfiles;
    private readonly IGenericRepository<DriverProfile, Guid> _driverProfiles;
    private readonly IAuditWriter _auditWriter;

    public UserManagementService(
        IIdentityGateway identityGateway,
        INotificationSender notificationSender,
        IGenericRepository<StudentProfile, Guid> studentProfiles,
        IGenericRepository<EmployeeProfile, Guid> employeeProfiles,
        IGenericRepository<DriverProfile, Guid> driverProfiles,
        IAuditWriter auditWriter)
    {
        _identityGateway = identityGateway;
        _notificationSender = notificationSender;
        _studentProfiles = studentProfiles;
        _employeeProfiles = employeeProfiles;
        _driverProfiles = driverProfiles;
        _auditWriter = auditWriter;
    }

    public async Task<OperationResult<Guid>> ProvisionAccountAsync(
        RegisterAccountRequestDto dto, string confirmationLinkBase, CancellationToken ct = default)
    {
        var roleName = dto.Role switch
        {
            RegistrationRole.Student => RoleNames.Student,
            RegistrationRole.Employee => RoleNames.Employee,
            RegistrationRole.Driver => RoleNames.Driver,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), "Unsupported registration role.")
        };

        if (dto.Role == RegistrationRole.Student && string.IsNullOrWhiteSpace(dto.StudentCode))
        {
            return OperationResult<Guid>.Failure(
                OperationResultStatus.ValidationError, "StudentCode is required for Student accounts.");
        }
        if (dto.Role == RegistrationRole.Employee && string.IsNullOrWhiteSpace(dto.EmployeeCode))
        {
            return OperationResult<Guid>.Failure(
                OperationResultStatus.ValidationError, "EmployeeCode is required for Employee accounts.");
        }
        if (dto.Role == RegistrationRole.Driver && string.IsNullOrWhiteSpace(dto.LicenseNumber))
        {
            return OperationResult<Guid>.Failure(
                OperationResultStatus.ValidationError, "LicenseNumber is required for Driver accounts.");
        }

        var password = string.IsNullOrEmpty(dto.Password) ? GenerateTemporaryPassword() : dto.Password;
        var registerResult = await _identityGateway.RegisterAccountAsync(
            dto.Email, password, dto.FirstName, dto.LastName, roleName, ct);
        if (!registerResult.IsSuccess || registerResult.Data is null)
        {
            return OperationResult<Guid>.Failure(registerResult.Status, registerResult.Errors.ToArray());
        }

        var userId = registerResult.Data.UserId;
        switch (dto.Role)
        {
            case RegistrationRole.Student:
                await _studentProfiles.AddAsync(
                    new StudentProfile { Id = Guid.NewGuid(), UserId = userId, StudentCode = dto.StudentCode! }, ct);
                await _studentProfiles.SaveChangesAsync(ct);
                break;
            case RegistrationRole.Employee:
                await _employeeProfiles.AddAsync(
                    new EmployeeProfile { Id = Guid.NewGuid(), UserId = userId, EmployeeCode = dto.EmployeeCode! }, ct);
                await _employeeProfiles.SaveChangesAsync(ct);
                break;
            case RegistrationRole.Driver:
                await _driverProfiles.AddAsync(
                    new DriverProfile { Id = Guid.NewGuid(), UserId = userId, LicenseNumber = dto.LicenseNumber! }, ct);
                await _driverProfiles.SaveChangesAsync(ct);
                break;
        }

        var confirmationLink = $"{confirmationLinkBase}?userId={userId}&token={Uri.EscapeDataString(registerResult.Data.EmailConfirmationToken)}";
        await _notificationSender.SendAccountConfirmationAsync(dto.Email, dto.FirstName, confirmationLink, ct);

        await _auditWriter.WriteAsync("Account.Provision", "ApplicationUser", userId.ToString(), null, $"Role={roleName}", ct);
        return OperationResult<Guid>.Success(userId, "Account provisioned. A confirmation email has been sent.");
    }

    private static string GenerateTemporaryPassword() => $"Tmp!{Guid.NewGuid():N}9a";
}

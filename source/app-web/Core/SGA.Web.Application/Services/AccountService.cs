using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Web.Application.Auditing;
using SGA.Web.Application.Dtos;
using SGA.Web.Application.Identity;
using SGA.Web.Application.Notifications;

namespace SGA.Web.Application.Services;

// Thin wrapper around SGA.Identity's account operations (via IIdentityGateway): adds the
// application-affinity role assignment (Student/Employee/Driver only, never TransportAdmin),
// creates the matching satellite profile, and dispatches the confirmation/reset notifications.
public class AccountService
{
    private readonly IIdentityGateway _identityGateway;
    private readonly INotificationSender _notificationSender;
    private readonly IGenericRepository<StudentProfile, Guid> _studentProfiles;
    private readonly IGenericRepository<EmployeeProfile, Guid> _employeeProfiles;
    private readonly IGenericRepository<DriverProfile, Guid> _driverProfiles;
    private readonly IAuditWriter _auditWriter;

    public AccountService(
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

    public async Task<OperationResult<Guid>> RegisterAsync(
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
                OperationResultStatus.ValidationError, "StudentCode is required for Student registrations.");
        }
        if (dto.Role == RegistrationRole.Employee && string.IsNullOrWhiteSpace(dto.EmployeeCode))
        {
            return OperationResult<Guid>.Failure(
                OperationResultStatus.ValidationError, "EmployeeCode is required for Employee registrations.");
        }
        if (dto.Role == RegistrationRole.Driver && string.IsNullOrWhiteSpace(dto.LicenseNumber))
        {
            return OperationResult<Guid>.Failure(
                OperationResultStatus.ValidationError, "LicenseNumber is required for Driver registrations.");
        }

        var registerResult = await _identityGateway.RegisterAsync(
            dto.Email, dto.Password, dto.FirstName, dto.LastName, roleName, ct);
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

        await _auditWriter.WriteAsync("Account.Register", "ApplicationUser", userId.ToString(), userId, $"Role={roleName}", ct);

        return OperationResult<Guid>.Success(userId, "Registration successful. Please check your email to confirm your account.");
    }

    public Task<OperationResult> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct = default) =>
        _identityGateway.ConfirmEmailAsync(userId, token, ct);

    public Task<OperationResult<TokenResult>> LoginAsync(string email, string password, CancellationToken ct = default) =>
        _identityGateway.LoginAsync(email, password, ct);

    public async Task<OperationResult> RequestPasswordResetAsync(
        string email, string resetLinkBase, CancellationToken ct = default)
    {
        var result = await _identityGateway.RequestPasswordResetAsync(email, ct);
        if (result.Data is not null)
        {
            var link = $"{resetLinkBase}?userId={result.Data.UserId}&token={Uri.EscapeDataString(result.Data.Token)}";
            await _notificationSender.SendPasswordResetAsync(email, string.Empty, link, ct);
        }

        // Always a generic message — never reveals whether the email exists.
        return OperationResult.Success("If that email exists, a password reset link has been sent.");
    }

    public Task<OperationResult> ResetPasswordAsync(
        Guid userId, string token, string newPassword, CancellationToken ct = default) =>
        _identityGateway.ResetPasswordAsync(userId, token, newPassword, ct);
}

using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Notifications;
using SGA.Desktop.Domain.Dtos;
using SGA.Desktop.Domain.Validation;

namespace SGA.Desktop.Infrastructure.Persistence.Validators;

public class DriverValidator : IDriverValidator
{
    private readonly DesktopAppDbContext _context;

    public DriverValidator(DesktopAppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationContext> ValidateForCreateAsync(CreateDriverDto dto, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        if (await _context.DriverProfiles.AnyAsync(d => d.LicenseNumber == dto.LicenseNumber, ct))
        {
            notifications.AddNotification("LicenseNumber", "A driver with this license number already exists.");
        }
        return notifications;
    }

    public async Task<NotificationContext> ValidateForUpdateAsync(Guid id, UpdateDriverDto dto, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        var duplicateLicense = await _context.DriverProfiles
            .AnyAsync(d => d.LicenseNumber == dto.LicenseNumber && d.Id != id, ct);
        if (duplicateLicense)
        {
            notifications.AddNotification("LicenseNumber", "Another driver already uses this license number.");
        }
        if (!await _context.DriverProfiles.AnyAsync(d => d.Id == id, ct))
        {
            notifications.AddNotification("NotFound", "Driver not found.");
        }
        return notifications;
    }

    public async Task<NotificationContext> ValidateForDeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        if (!await _context.DriverProfiles.AnyAsync(d => d.Id == id, ct))
        {
            notifications.AddNotification("NotFound", "Driver not found.");
        }
        return notifications;
    }
}

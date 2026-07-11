using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Notifications;
using SGA.Desktop.Domain.Dtos;
using SGA.Desktop.Domain.Validation;

namespace SGA.Desktop.Infrastructure.Persistence.Validators;

public class BusValidator : IBusValidator
{
    private readonly DesktopAppDbContext _context;

    public BusValidator(DesktopAppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationContext> ValidateForCreateAsync(CreateBusDto dto, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();

        if (dto.Capacity <= 0)
        {
            notifications.AddNotification("Capacity", "Capacity must be greater than zero.");
        }

        if (await _context.Buses.AnyAsync(b => b.PlateNumber == dto.PlateNumber, ct))
        {
            notifications.AddNotification("PlateNumber", "A bus with this plate number already exists.");
        }

        return notifications;
    }

    public async Task<NotificationContext> ValidateForUpdateAsync(Guid id, UpdateBusDto dto, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();

        if (dto.Capacity <= 0)
        {
            notifications.AddNotification("Capacity", "Capacity must be greater than zero.");
        }
        if (!await _context.Buses.AnyAsync(b => b.Id == id, ct))
        {
            notifications.AddNotification("NotFound", "Bus not found.");
        }

        return notifications;
    }

    public async Task<NotificationContext> ValidateForDeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var notifications = new NotificationContext();
        if (!await _context.Buses.AnyAsync(b => b.Id == id, ct))
        {
            notifications.AddNotification("NotFound", "Bus not found.");
        }
        return notifications;
    }
}

using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Auditing;
using SGA.Desktop.Application.Persistence;
using SGA.Desktop.Domain.Dtos;
using SGA.Desktop.Domain.Validation;

namespace SGA.Desktop.Application.Services;

public class FleetManagementService
{
    private readonly IBusValidator _validator;
    private readonly IBusRepository _repository;
    private readonly IAuditWriter _auditWriter;

    public FleetManagementService(IBusValidator validator, IBusRepository repository, IAuditWriter auditWriter)
    {
        _validator = validator;
        _repository = repository;
        _auditWriter = auditWriter;
    }

    public Task<IReadOnlyList<Bus>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default) =>
        _repository.GetAllAsync(includeInactive, ct);

    public Task<Bus?> GetByIdAsync(Guid id, CancellationToken ct = default) => _repository.GetByIdAsync(id, ct);

    public async Task<OperationResult<Bus>> CreateAsync(CreateBusDto dto, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateForCreateAsync(dto, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Bus>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var bus = new Bus
        {
            Id = Guid.NewGuid(),
            PlateNumber = dto.PlateNumber,
            Model = dto.Model,
            Year = dto.Year,
            Capacity = dto.Capacity,
            BusStatus = SGA.SharedKernel.Domain.Enums.BusStatus.Active
        };
        await _repository.AddAsync(bus, ct);
        await _repository.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Bus.Create", nameof(Bus), bus.Id.ToString(), null, null, ct);
        return OperationResult<Bus>.Success(bus);
    }

    public async Task<OperationResult<Bus>> UpdateAsync(Guid id, UpdateBusDto dto, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateForUpdateAsync(id, dto, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Bus>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var bus = await _repository.GetByIdAsync(id, ct);
        if (bus is null)
        {
            return OperationResult<Bus>.Failure(OperationResultStatus.NotFound, "Bus not found.");
        }

        bus.Model = dto.Model;
        bus.Year = dto.Year;
        bus.Capacity = dto.Capacity;
        bus.BusStatus = dto.BusStatus;
        await _repository.UpdateAsync(bus, ct);
        await _repository.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Bus.Update", nameof(Bus), bus.Id.ToString(), null, null, ct);
        return OperationResult<Bus>.Success(bus);
    }

    public async Task<OperationResult> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateForDeactivateAsync(id, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        await _repository.DeactivateAsync(id, ct);
        await _repository.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Bus.Deactivate", nameof(Bus), id.ToString(), null, null, ct);
        return OperationResult.Success("Bus deactivated.");
    }
}

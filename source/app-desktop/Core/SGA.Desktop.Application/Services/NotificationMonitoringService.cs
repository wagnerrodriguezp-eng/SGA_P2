using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Desktop.Application.Services;

// Read-only — NotificationMessage rows are written exclusively by SGA.Shared.Messaging's outbox
// dispatch/retry pipeline. This service exists purely so the Transport Admin can monitor delivery
// status/attempt counts, never to create or edit messages.
public class NotificationMonitoringService
{
    private readonly IGenericRepository<NotificationMessage, Guid> _repository;

    public NotificationMonitoringService(IGenericRepository<NotificationMessage, Guid> repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<NotificationMessage>> GetAllAsync(
        bool includeInactive = false, CancellationToken ct = default) =>
        _repository.GetAllAsync(includeInactive, ct);

    public Task<NotificationMessage?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _repository.GetByIdAsync(id, ct);
}

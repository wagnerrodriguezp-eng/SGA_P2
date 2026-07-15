using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Desktop.Application.Auditing;
using SGA.Desktop.Application.Persistence;

namespace SGA.Desktop.Application.Services;

public class IncidentManagementService
{
    private readonly IIncidentRepository _repository;
    private readonly IAuditWriter _auditWriter;

    public IncidentManagementService(IIncidentRepository repository, IAuditWriter auditWriter)
    {
        _repository = repository;
        _auditWriter = auditWriter;
    }

    public Task<IReadOnlyList<Incident>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default) =>
        _repository.GetAllAsync(includeInactive, ct);

    public Task<Incident?> GetByIdAsync(Guid id, CancellationToken ct = default) => _repository.GetByIdAsync(id, ct);

    public Task<IReadOnlyList<Incident>> GetByTripIdAsync(Guid tripId, CancellationToken ct = default) =>
        _repository.GetByTripIdAsync(tripId, ct);

    public async Task<OperationResult<Incident>> UpdateStatusAsync(
        Guid id, IncidentStatus status, CancellationToken ct = default)
    {
        var incident = await _repository.GetByIdAsync(id, ct);
        if (incident is null)
        {
            return OperationResult<Incident>.Failure(OperationResultStatus.NotFound, "Incident not found.");
        }

        incident.IncidentStatus = status;
        await _repository.UpdateAsync(incident, ct);
        await _repository.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync($"Incident.{status}", nameof(Incident), incident.Id.ToString(), null, null, ct);
        return OperationResult<Incident>.Success(incident);
    }
}

using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Desktop.Application.Persistence;

public interface IIncidentRepository : IGenericRepository<Incident, Guid>
{
    Task<IReadOnlyList<Incident>> GetByTripIdAsync(Guid tripId, CancellationToken ct = default);
}

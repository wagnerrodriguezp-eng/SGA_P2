using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Entities;
using SGA.Web.Application.Persistence;

namespace SGA.Web.Infrastructure.Persistence.Repositories;

public class IncidentRepository : GenericRepository<Incident, Guid>, IIncidentRepository
{
    public IncidentRepository(WebAppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Incident>> GetByTripIdAsync(Guid tripId, CancellationToken ct = default) =>
        await Context.Incidents.Where(i => i.TripId == tripId).ToListAsync(ct);
}

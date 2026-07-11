using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Entities;
using SGA.Web.Application.Persistence;

namespace SGA.Web.Infrastructure.Persistence.Repositories;

public class TripRepository : GenericRepository<Trip, Guid>, ITripRepository
{
    public TripRepository(WebAppDbContext context) : base(context)
    {
    }

    public async Task<Trip?> GetWithConcurrencyTokenAsync(Guid id, CancellationToken ct = default)
    {
        // Always hands back a fresh value — if the caller's retry loop already has this entity
        // tracked from a failed attempt, reload it instead of returning stale in-memory state.
        var tracked = Context.ChangeTracker.Entries<Trip>().FirstOrDefault(e => e.Entity.Id.Equals(id));
        if (tracked is not null)
        {
            await tracked.ReloadAsync(ct);
            return tracked.Entity;
        }

        return await Context.Trips.FirstOrDefaultAsync(t => t.Id.Equals(id), ct);
    }

    public async Task<IReadOnlyList<Trip>> GetAssignedToDriverTodayAsync(Guid driverUserId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await Context.Trips
            .Where(t => t.DriverUserId == driverUserId && t.TripDate == today)
            .ToListAsync(ct);
    }
}

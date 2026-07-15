using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Web.Application.Persistence;

namespace SGA.Web.Infrastructure.Persistence.Repositories;

public class AuthorizationRepository : GenericRepository<Authorization, Guid>, IAuthorizationRepository
{
    public AuthorizationRepository(WebAppDbContext context) : base(context)
    {
    }

    public async Task<Authorization?> GetActiveForUserAsync(Guid userId, CancellationToken ct = default)
    {
        // EF's identity map would otherwise hand back the same already-tracked (and already
        // decremented) instance on a retry within the same request — reload it explicitly so
        // each attempt re-reads the current Balance/RowVersion instead of stale in-memory state.
        var tracked = Context.ChangeTracker.Entries<Authorization>()
            .FirstOrDefault(e => e.Entity.UserId == userId && e.Entity.AuthorizationStatus == AuthorizationStatus.Active);
        if (tracked is not null)
        {
            await tracked.ReloadAsync(ct);
            return tracked.Entity.AuthorizationStatus == AuthorizationStatus.Active ? tracked.Entity : null;
        }

        return await Context.Authorizations
            .Where(a => a.UserId == userId && a.AuthorizationStatus == AuthorizationStatus.Active)
            .FirstOrDefaultAsync(ct);
    }
}

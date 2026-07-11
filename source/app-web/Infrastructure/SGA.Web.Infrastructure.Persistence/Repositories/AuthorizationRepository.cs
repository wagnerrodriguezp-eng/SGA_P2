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

    public async Task<Authorization?> GetActiveForUserAsync(Guid userId, CancellationToken ct = default) =>
        await Context.Authorizations
            .Where(a => a.UserId == userId && a.AuthorizationStatus == AuthorizationStatus.Active)
            .FirstOrDefaultAsync(ct);
}

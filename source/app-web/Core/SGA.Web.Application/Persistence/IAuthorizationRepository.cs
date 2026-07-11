using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Web.Application.Persistence;

public interface IAuthorizationRepository : IGenericRepository<Authorization, Guid>
{
    Task<Authorization?> GetActiveForUserAsync(Guid userId, CancellationToken ct = default);
}

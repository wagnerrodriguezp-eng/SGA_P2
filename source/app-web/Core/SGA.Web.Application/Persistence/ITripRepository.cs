using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Web.Application.Persistence;

public interface ITripRepository : IGenericRepository<Trip, Guid>
{
    // Returns the entity ready for an update against its RowVersion concurrency token — always a
    // fresh read (bypassing any stale tracked instance), used by the capacity-increment retry loop.
    Task<Trip?> GetWithConcurrencyTokenAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Trip>> GetAssignedToDriverTodayAsync(Guid driverUserId, CancellationToken ct = default);
}

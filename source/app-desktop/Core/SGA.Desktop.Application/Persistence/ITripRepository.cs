using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Desktop.Application.Persistence;

public interface ITripRepository : IGenericRepository<Trip, Guid>
{
    Task<bool> HasOverlappingAssignmentAsync(
        Guid busId, Guid driverUserId, DateOnly tripDate, TimeSpan departureTime, CancellationToken ct = default);

    Task<IReadOnlyList<Trip>> GetAllForDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}

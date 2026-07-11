using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Notifications;
using SGA.Web.Application.Auditing;
using SGA.Web.Application.Persistence;
using SGA.Web.Domain.Validation;

namespace SGA.Web.Application.UnitTests.Fixtures;

// In-memory fakes for the Application layer's own unit tests — mirrors the source SAD's reference
// pattern of an in-memory repository substitute, never a real DbContext.
internal class FakeAccessValidator : IAccessValidator
{
    public Task<NotificationContext> ValidateAccessAsync(Guid userId, Guid tripId, CancellationToken ct = default) =>
        Task.FromResult(new NotificationContext());
}

internal class FakeTripRepository : ITripRepository
{
    private readonly Trip _trip;
    private readonly int _failuresBeforeSuccess;

    public int SaveChangesCallCount { get; private set; }

    public FakeTripRepository(Trip trip, int failuresBeforeSuccess = 0)
    {
        _trip = trip;
        _failuresBeforeSuccess = failuresBeforeSuccess;
    }

    public Task<Trip?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Trip?>(_trip);

    public Task<IReadOnlyList<Trip>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Trip>>(new List<Trip> { _trip });

    public Task AddAsync(Trip entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(Trip entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeactivateAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCallCount++;
        if (SaveChangesCallCount <= _failuresBeforeSuccess)
        {
            throw new ConcurrencyConflictException("Simulated RowVersion conflict.");
        }
        return Task.FromResult(1);
    }

    public Task<Trip?> GetWithConcurrencyTokenAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Trip?>(_trip);

    public Task<IReadOnlyList<Trip>> GetAssignedToDriverTodayAsync(Guid driverUserId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Trip>>(new List<Trip>());
}

internal class FakeAuthorizationRepository : IAuthorizationRepository
{
    private readonly Authorization _authorization;

    public FakeAuthorizationRepository(Authorization authorization)
    {
        _authorization = authorization;
    }

    public Task<Authorization?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Authorization?>(_authorization);

    public Task<IReadOnlyList<Authorization>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Authorization>>(new List<Authorization> { _authorization });

    public Task AddAsync(Authorization entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(Authorization entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeactivateAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(1);

    public Task<Authorization?> GetActiveForUserAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult<Authorization?>(_authorization);
}

internal class FakeUsageRecordRepository : IGenericRepository<UsageRecord, Guid>
{
    public List<UsageRecord> Added { get; } = new();

    public Task<UsageRecord?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Added.FirstOrDefault(u => u.Id == id));

    public Task<IReadOnlyList<UsageRecord>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<UsageRecord>>(Added);

    public Task AddAsync(UsageRecord entity, CancellationToken ct = default)
    {
        Added.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UsageRecord entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeactivateAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(1);
}

internal class FakeAuditWriter : IAuditWriter
{
    public List<string> Actions { get; } = new();

    public Task WriteAsync(string action, string entityName, string? entityId, Guid? userId, string? details, CancellationToken ct = default)
    {
        Actions.Add(action);
        return Task.CompletedTask;
    }
}

using SGA.SharedKernel.Domain.Common;

namespace SGA.SharedKernel.Application.Persistence;

public interface IGenericRepository<TEntity, TId> where TEntity : BaseEntity<TId>
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);
    Task DeactivateAsync(TId id, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

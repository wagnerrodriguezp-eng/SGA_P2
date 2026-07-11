using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Common;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.Desktop.Infrastructure.Persistence.Repositories;

public class GenericRepository<TEntity, TId> : IGenericRepository<TEntity, TId>
    where TEntity : BaseEntity<TId>
{
    protected readonly DesktopAppDbContext Context;

    public GenericRepository(DesktopAppDbContext context)
    {
        Context = context;
    }

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default) =>
        await Context.Set<TEntity>().FindAsync(new object?[] { id }, ct);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = Context.Set<TEntity>().AsQueryable();
        if (includeInactive)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.ToListAsync(ct);
    }

    public Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        Context.Set<TEntity>().Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        Context.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeactivateAsync(TId id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is null)
        {
            return;
        }
        entity.RecordStatus = RecordStatus.Inactive;
        Context.Set<TEntity>().Update(entity);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await Context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(
                "The record was modified by another request before this change could be saved.", ex);
        }
    }
}

using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Desktop.Application.Services;

public class AuditQueryService
{
    private readonly IGenericRepository<AuditLog, Guid> _auditLogs;

    public AuditQueryService(IGenericRepository<AuditLog, Guid> auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task<IReadOnlyList<AuditLog>> SearchAsync(
        DateTime? from, DateTime? to, Guid? userId, string? action, CancellationToken ct = default)
    {
        var all = await _auditLogs.GetAllAsync(includeInactive: true, ct);
        return all
            .Where(a => !from.HasValue || a.TimestampUtc >= from.Value)
            .Where(a => !to.HasValue || a.TimestampUtc <= to.Value)
            .Where(a => !userId.HasValue || a.UserId == userId.Value)
            .Where(a => string.IsNullOrEmpty(action) || a.Action.Contains(action, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.TimestampUtc)
            .ToList();
    }
}

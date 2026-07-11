using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Entities;
using SGA.Web.Application.Auditing;

namespace SGA.Web.Infrastructure.Shared.Auditing;

// Writes to the AuditLog table under normal conditions. If the database itself is unreachable,
// falls back to a rolling local text log so the event is never silently lost — reconciled back
// into AuditLog manually/via a follow-up job if needed.
public class AuditWriter : IAuditWriter
{
    private readonly IGenericRepository<AuditLog, Guid> _auditLogs;

    public AuditWriter(IGenericRepository<AuditLog, Guid> auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task WriteAsync(
        string action, string entityName, string? entityId, Guid? userId, string? details, CancellationToken ct = default)
    {
        var entry = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            TimestampUtc = DateTime.UtcNow,
            Details = details
        };

        try
        {
            await _auditLogs.AddAsync(entry, ct);
            await _auditLogs.SaveChangesAsync(ct);
        }
        catch (Exception)
        {
            await WriteFallbackFileAsync(entry, ct);
        }
    }

    private static async Task WriteFallbackFileAsync(AuditLog entry, CancellationToken ct)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"audit-fallback-{DateTime.UtcNow:yyyyMMdd}.txt");
        var line = $"{entry.TimestampUtc:O}\t{entry.Action}\t{entry.EntityName}\t{entry.EntityId}\t{entry.UserId}\t{entry.Details}{Environment.NewLine}";
        await File.AppendAllTextAsync(path, line, ct);
    }
}

namespace SGA.Web.Application.Auditing;

public interface IAuditWriter
{
    Task WriteAsync(
        string action, string entityName, string? entityId, Guid? userId, string? details, CancellationToken ct = default);
}

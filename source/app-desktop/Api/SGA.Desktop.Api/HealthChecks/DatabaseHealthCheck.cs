using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SGA.Desktop.Infrastructure.Persistence;

namespace SGA.Desktop.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly DesktopAppDbContext _context;

    public DatabaseHealthCheck(DesktopAppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default) =>
        await _context.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Cannot connect to the database.");
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SGA.Web.Infrastructure.Persistence;

namespace SGA.Web.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly WebAppDbContext _context;

    public DatabaseHealthCheck(WebAppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default) =>
        await _context.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Cannot connect to the database.");
}

using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Web.Application.Services;

// Read-only: routes, stops, schedules — returns Shared Kernel entities directly (no DTO), per
// entity-catalog rule 4.
public class ScheduleQueryService
{
    private readonly IGenericRepository<Route, Guid> _routes;
    private readonly IGenericRepository<Stop, Guid> _stops;
    private readonly IGenericRepository<Schedule, Guid> _schedules;

    public ScheduleQueryService(
        IGenericRepository<Route, Guid> routes,
        IGenericRepository<Stop, Guid> stops,
        IGenericRepository<Schedule, Guid> schedules)
    {
        _routes = routes;
        _stops = stops;
        _schedules = schedules;
    }

    public Task<IReadOnlyList<Route>> GetRoutesAsync(CancellationToken ct = default) =>
        _routes.GetAllAsync(ct: ct);

    public async Task<IReadOnlyList<Stop>> GetStopsForRouteAsync(Guid routeId, CancellationToken ct = default)
    {
        var stops = await _stops.GetAllAsync(ct: ct);
        return stops.Where(s => s.RouteId == routeId).OrderBy(s => s.Order).ToList();
    }

    public async Task<IReadOnlyList<Schedule>> GetSchedulesAsync(Guid? routeId, CancellationToken ct = default)
    {
        var schedules = await _schedules.GetAllAsync(ct: ct);
        return (routeId.HasValue ? schedules.Where(s => s.RouteId == routeId.Value) : schedules).ToList();
    }
}

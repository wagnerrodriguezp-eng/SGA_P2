using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Auditing;
using SGA.Desktop.Domain.Dtos;
using SGA.Desktop.Domain.Validation;

namespace SGA.Desktop.Application.Services;

public class RouteSchedulingService
{
    private readonly IRouteScheduleValidator _validator;
    private readonly IGenericRepository<Route, Guid> _routes;
    private readonly IGenericRepository<Stop, Guid> _stops;
    private readonly IGenericRepository<Schedule, Guid> _schedules;
    private readonly IAuditWriter _auditWriter;

    public RouteSchedulingService(
        IRouteScheduleValidator validator,
        IGenericRepository<Route, Guid> routes,
        IGenericRepository<Stop, Guid> stops,
        IGenericRepository<Schedule, Guid> schedules,
        IAuditWriter auditWriter)
    {
        _validator = validator;
        _routes = routes;
        _stops = stops;
        _schedules = schedules;
        _auditWriter = auditWriter;
    }

    public Task<IReadOnlyList<Route>> GetAllRoutesAsync(bool includeInactive = false, CancellationToken ct = default) =>
        _routes.GetAllAsync(includeInactive, ct);

    public async Task<IReadOnlyList<Stop>> GetStopsForRouteAsync(Guid routeId, CancellationToken ct = default)
    {
        var stops = await _stops.GetAllAsync(ct: ct);
        return stops.Where(s => s.RouteId == routeId).OrderBy(s => s.Order).ToList();
    }

    public async Task<IReadOnlyList<Schedule>> GetSchedulesForRouteAsync(Guid routeId, CancellationToken ct = default)
    {
        var schedules = await _schedules.GetAllAsync(ct: ct);
        return schedules.Where(s => s.RouteId == routeId).ToList();
    }

    public async Task<OperationResult<Route>> CreateRouteAsync(CreateRouteDto dto, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateRouteForCreateAsync(dto, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Route>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var route = new Route
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            RouteStatus = SGA.SharedKernel.Domain.Enums.RouteStatus.Active
        };
        await _routes.AddAsync(route, ct);
        await _routes.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Route.Create", nameof(Route), route.Id.ToString(), null, null, ct);
        return OperationResult<Route>.Success(route);
    }

    public async Task<OperationResult<Route>> UpdateRouteAsync(Guid id, UpdateRouteDto dto, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateRouteForUpdateAsync(id, dto, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Route>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var route = await _routes.GetByIdAsync(id, ct);
        if (route is null)
        {
            return OperationResult<Route>.Failure(OperationResultStatus.NotFound, "Route not found.");
        }

        route.Name = dto.Name;
        route.Description = dto.Description;
        route.RouteStatus = dto.RouteStatus;
        await _routes.UpdateAsync(route, ct);
        await _routes.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Route.Update", nameof(Route), route.Id.ToString(), null, null, ct);
        return OperationResult<Route>.Success(route);
    }

    public async Task<OperationResult> DeactivateRouteAsync(Guid id, CancellationToken ct = default)
    {
        await _routes.DeactivateAsync(id, ct);
        await _routes.SaveChangesAsync(ct);
        await _auditWriter.WriteAsync("Route.Deactivate", nameof(Route), id.ToString(), null, null, ct);
        return OperationResult.Success("Route deactivated.");
    }

    public async Task<OperationResult<Stop>> CreateStopAsync(CreateStopDto dto, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateStopForCreateAsync(dto, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Stop>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var stop = new Stop
        {
            Id = Guid.NewGuid(),
            RouteId = dto.RouteId,
            Name = dto.Name,
            Order = dto.Order,
            StopStatus = SGA.SharedKernel.Domain.Enums.StopStatus.Active
        };
        await _stops.AddAsync(stop, ct);
        await _stops.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Stop.Create", nameof(Stop), stop.Id.ToString(), null, null, ct);
        return OperationResult<Stop>.Success(stop);
    }

    public async Task<OperationResult<Stop>> UpdateStopAsync(Guid routeId, Guid id, UpdateStopDto dto, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateStopForUpdateAsync(id, dto, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Stop>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var stop = await _stops.GetByIdAsync(id, ct);
        if (stop is null || stop.RouteId != routeId)
        {
            return OperationResult<Stop>.Failure(OperationResultStatus.NotFound, "Stop not found.");
        }

        stop.Name = dto.Name;
        stop.Order = dto.Order;
        stop.StopStatus = dto.StopStatus;
        await _stops.UpdateAsync(stop, ct);
        await _stops.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Stop.Update", nameof(Stop), stop.Id.ToString(), null, null, ct);
        return OperationResult<Stop>.Success(stop);
    }

    public async Task<OperationResult> DeactivateStopAsync(Guid routeId, Guid id, CancellationToken ct = default)
    {
        var stop = await _stops.GetByIdAsync(id, ct);
        if (stop is null || stop.RouteId != routeId)
        {
            return OperationResult.Failure(OperationResultStatus.NotFound, "Stop not found.");
        }

        await _stops.DeactivateAsync(id, ct);
        await _stops.SaveChangesAsync(ct);
        await _auditWriter.WriteAsync("Stop.Deactivate", nameof(Stop), id.ToString(), null, null, ct);
        return OperationResult.Success("Stop deactivated.");
    }

    public async Task<OperationResult<Schedule>> CreateScheduleAsync(CreateScheduleDto dto, CancellationToken ct = default)
    {
        var notifications = await _validator.ValidateScheduleForCreateAsync(dto, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Schedule>.Failure(
                OperationResultStatus.ValidationError, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            RouteId = dto.RouteId,
            DepartureTime = dto.DepartureTime,
            DaysOfWeekMask = dto.DaysOfWeekMask,
            ScheduleStatus = SGA.SharedKernel.Domain.Enums.ScheduleStatus.Active
        };
        await _schedules.AddAsync(schedule, ct);
        await _schedules.SaveChangesAsync(ct);

        await _auditWriter.WriteAsync("Schedule.Create", nameof(Schedule), schedule.Id.ToString(), null, null, ct);
        return OperationResult<Schedule>.Success(schedule);
    }
}

using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Enums;
using SGA.Desktop.Application.Persistence;
using SGA.Desktop.Application.Reporting;
using SGA.Desktop.Domain.Dtos;

namespace SGA.Desktop.Infrastructure.Persistence.Repositories;

public class ReportingRepository : IReportingRepository
{
    private readonly DesktopAppDbContext _context;

    public ReportingRepository(DesktopAppDbContext context)
    {
        _context = context;
    }

    public async Task<UsageReportResult> GetUsageReportAsync(ReportFilterDto filter, CancellationToken ct = default)
    {
        var tripIds = await GetTripIdsInRangeAsync(filter, ct);
        var results = await _context.UsageRecords
            .Where(u => tripIds.Contains(u.TripId))
            .Select(u => u.AccessResult)
            .ToListAsync(ct);

        return new UsageReportResult(
            results.Count,
            results.Count(r => r == AccessResult.Granted),
            results.Count(r => r == AccessResult.DeniedExpired),
            results.Count(r => r == AccessResult.DeniedNoBalance),
            results.Count(r => r == AccessResult.DeniedNoCapacity),
            results.Count(r => r == AccessResult.DeniedUnauthorized));
    }

    public async Task<OccupancyReportResult> GetOccupancyReportAsync(ReportFilterDto filter, CancellationToken ct = default)
    {
        var tripIds = await GetTripIdsInRangeAsync(filter, ct);
        var trips = await _context.Trips.Where(t => tripIds.Contains(t.Id)).ToListAsync(ct);
        if (trips.Count == 0)
        {
            return new OccupancyReportResult(0, 0);
        }

        var average = trips.Average(t => t.MaxCapacitySnapshot == 0 ? 0 : (double)t.CapacityUsed / t.MaxCapacitySnapshot * 100);
        return new OccupancyReportResult(trips.Count, Math.Round(average, 2));
    }

    public async Task<PunctualityReportResult> GetPunctualityReportAsync(ReportFilterDto filter, CancellationToken ct = default)
    {
        var tripIds = await GetTripIdsInRangeAsync(filter, ct);
        var trips = await _context.Trips
            .Where(t => tripIds.Contains(t.Id) && t.StartedAtUtc != null)
            .ToListAsync(ct);
        if (trips.Count == 0)
        {
            return new PunctualityReportResult(0, 0, 0, 0);
        }

        var scheduleIds = trips.Select(t => t.ScheduleId).Distinct().ToList();
        var departureTimesById = await _context.Schedules
            .Where(s => scheduleIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.DepartureTime, ct);

        var onTimeCount = 0;
        foreach (var trip in trips)
        {
            if (!departureTimesById.TryGetValue(trip.ScheduleId, out var departureTime))
            {
                continue;
            }

            var scheduledUtc = DateTime.SpecifyKind(
                trip.TripDate.ToDateTime(TimeOnly.FromTimeSpan(departureTime)), DateTimeKind.Utc);
            if (Math.Abs((trip.StartedAtUtc!.Value - scheduledUtc).TotalMinutes) <= 10)
            {
                onTimeCount++;
            }
        }

        var delayedCount = trips.Count - onTimeCount;
        return new PunctualityReportResult(
            trips.Count, onTimeCount, delayedCount, Math.Round((double)onTimeCount / trips.Count * 100, 2));
    }

    public async Task<RevenueReportResult> GetRevenueReportAsync(ReportFilterDto filter, CancellationToken ct = default)
    {
        var from = DateTime.SpecifyKind(filter.DateFrom.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var to = DateTime.SpecifyKind(filter.DateTo.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        var authorizations = await _context.Authorizations
            .Where(a => a.IssuedAtUtc >= from && a.IssuedAtUtc <= to)
            .Select(a => a.AuthorizationType)
            .ToListAsync(ct);

        return new RevenueReportResult(
            authorizations.Count(a => a == AuthorizationType.MonthlyTicket),
            authorizations.Count(a => a == AuthorizationType.RechargeableCard));
    }

    private async Task<List<Guid>> GetTripIdsInRangeAsync(ReportFilterDto filter, CancellationToken ct)
    {
        var query = _context.Trips.Where(t => t.TripDate >= filter.DateFrom && t.TripDate <= filter.DateTo);
        if (filter.RouteId.HasValue)
        {
            var scheduleIds = await _context.Schedules
                .Where(s => s.RouteId == filter.RouteId.Value)
                .Select(s => s.Id)
                .ToListAsync(ct);
            query = query.Where(t => scheduleIds.Contains(t.ScheduleId));
        }

        return await query.Select(t => t.Id).ToListAsync(ct);
    }
}

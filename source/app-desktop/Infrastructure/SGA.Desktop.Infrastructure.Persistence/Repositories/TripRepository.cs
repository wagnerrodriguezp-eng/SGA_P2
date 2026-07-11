using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Desktop.Application.Persistence;

namespace SGA.Desktop.Infrastructure.Persistence.Repositories;

public class TripRepository : GenericRepository<Trip, Guid>, ITripRepository
{
    // The spec's "overlapping schedule departure window" is under-specified (a Trip has a single
    // DepartureTime, no duration) — interpreted here as: the same bus/driver may not have another
    // active trip on the same date whose scheduled departure falls within this buffer.
    private static readonly TimeSpan OverlapBuffer = TimeSpan.FromMinutes(60);

    public TripRepository(DesktopAppDbContext context) : base(context)
    {
    }

    public async Task<bool> HasOverlappingAssignmentAsync(
        Guid busId, Guid driverUserId, DateOnly tripDate, TimeSpan departureTime, CancellationToken ct = default)
    {
        var candidateTrips = await Context.Trips
            .Where(t => t.TripDate == tripDate && t.TripStatus != TripStatus.Cancelled &&
                        (t.BusId == busId || t.DriverUserId == driverUserId))
            .ToListAsync(ct);

        if (candidateTrips.Count == 0)
        {
            return false;
        }

        var scheduleIds = candidateTrips.Select(t => t.ScheduleId).Distinct().ToList();
        var departureTimesById = await Context.Schedules
            .Where(s => scheduleIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.DepartureTime, ct);

        return candidateTrips.Any(t =>
            departureTimesById.TryGetValue(t.ScheduleId, out var otherDeparture) &&
            Math.Abs((otherDeparture - departureTime).TotalMinutes) < OverlapBuffer.TotalMinutes);
    }

    public async Task<IReadOnlyList<Trip>> GetAllForDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default) =>
        await Context.Trips.Where(t => t.TripDate >= from && t.TripDate <= to).ToListAsync(ct);
}

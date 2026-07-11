using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Desktop.Application.UnitTests.Fixtures;
using SGA.Desktop.Infrastructure.Persistence.Repositories;
using SGA.Desktop.Infrastructure.Persistence.Validators;

namespace SGA.Desktop.Application.UnitTests.Validators;

public class TripAssignmentValidatorTests
{
    [Fact]
    public async Task ValidateAssignmentAsync_NoConflict_ReturnsNoNotifications()
    {
        await using var db = TestDbContextFactory.Create();
        var busId = Guid.NewGuid();
        var driverUserId = Guid.NewGuid();
        var routeId = Guid.NewGuid();

        db.Buses.Add(new Bus { Id = busId, PlateNumber = "P-001", Capacity = 40, BusStatus = BusStatus.Active });
        db.DriverProfiles.Add(new DriverProfile { Id = Guid.NewGuid(), UserId = driverUserId, LicenseNumber = "L-001" });
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(), RouteId = routeId, DepartureTime = new TimeSpan(8, 0, 0),
            DaysOfWeekMask = 31, ScheduleStatus = ScheduleStatus.Active
        };
        db.Schedules.Add(schedule);
        await db.SaveChangesAsync();

        var validator = new TripAssignmentValidator(db, new TripRepository(db));
        var result = await validator.ValidateAssignmentAsync(schedule.Id, busId, driverUserId, DateOnly.FromDateTime(DateTime.UtcNow));

        Assert.False(result.HasNotifications);
    }

    [Fact]
    public async Task ValidateAssignmentAsync_BusAlreadyAssignedWithinBuffer_ReturnsOverlap()
    {
        await using var db = TestDbContextFactory.Create();
        var busId = Guid.NewGuid();
        var driverUserId = Guid.NewGuid();
        var otherDriverUserId = Guid.NewGuid();
        var routeId = Guid.NewGuid();
        var tripDate = DateOnly.FromDateTime(DateTime.UtcNow);

        db.Buses.Add(new Bus { Id = busId, PlateNumber = "P-002", Capacity = 40, BusStatus = BusStatus.Active });
        db.DriverProfiles.Add(new DriverProfile { Id = Guid.NewGuid(), UserId = driverUserId, LicenseNumber = "L-002" });
        db.DriverProfiles.Add(new DriverProfile { Id = Guid.NewGuid(), UserId = otherDriverUserId, LicenseNumber = "L-003" });

        var existingSchedule = new Schedule
        {
            Id = Guid.NewGuid(), RouteId = routeId, DepartureTime = new TimeSpan(8, 0, 0),
            DaysOfWeekMask = 31, ScheduleStatus = ScheduleStatus.Active
        };
        var newSchedule = new Schedule
        {
            Id = Guid.NewGuid(), RouteId = routeId, DepartureTime = new TimeSpan(8, 30, 0),
            DaysOfWeekMask = 31, ScheduleStatus = ScheduleStatus.Active
        };
        db.Schedules.AddRange(existingSchedule, newSchedule);

        db.Trips.Add(new Trip
        {
            Id = Guid.NewGuid(), ScheduleId = existingSchedule.Id, BusId = busId, DriverUserId = otherDriverUserId,
            TripDate = tripDate, TripStatus = TripStatus.Scheduled, MaxCapacitySnapshot = 40, CapacityUsed = 0,
            RowVersion = new byte[8]
        });
        await db.SaveChangesAsync();

        var validator = new TripAssignmentValidator(db, new TripRepository(db));
        // Same bus, 30 minutes apart — within the 60-minute overlap buffer.
        var result = await validator.ValidateAssignmentAsync(newSchedule.Id, busId, driverUserId, tripDate);

        Assert.True(result.HasNotifications);
        Assert.Equal("Overlap", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateAssignmentAsync_InactiveBus_ReturnsBusIdNotification()
    {
        await using var db = TestDbContextFactory.Create();
        var busId = Guid.NewGuid();
        var driverUserId = Guid.NewGuid();
        var routeId = Guid.NewGuid();

        db.Buses.Add(new Bus { Id = busId, PlateNumber = "P-004", Capacity = 40, BusStatus = BusStatus.OutOfService });
        db.DriverProfiles.Add(new DriverProfile { Id = Guid.NewGuid(), UserId = driverUserId, LicenseNumber = "L-004" });
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(), RouteId = routeId, DepartureTime = new TimeSpan(9, 0, 0),
            DaysOfWeekMask = 31, ScheduleStatus = ScheduleStatus.Active
        };
        db.Schedules.Add(schedule);
        await db.SaveChangesAsync();

        var validator = new TripAssignmentValidator(db, new TripRepository(db));
        var result = await validator.ValidateAssignmentAsync(schedule.Id, busId, driverUserId, DateOnly.FromDateTime(DateTime.UtcNow));

        Assert.True(result.HasNotifications);
        Assert.Equal("BusId", result.Notifications.First().Key);
    }
}

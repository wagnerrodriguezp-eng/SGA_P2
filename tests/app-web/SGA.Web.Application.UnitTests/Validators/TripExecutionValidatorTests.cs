using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Web.Application.UnitTests.Fixtures;
using SGA.Web.Infrastructure.Persistence.Validators;

namespace SGA.Web.Application.UnitTests.Validators;

public class TripExecutionValidatorTests
{
    private static Trip NewTrip(Guid driverUserId, TripStatus status) => new()
    {
        Id = Guid.NewGuid(),
        ScheduleId = Guid.NewGuid(),
        BusId = Guid.NewGuid(),
        DriverUserId = driverUserId,
        TripDate = DateOnly.FromDateTime(DateTime.UtcNow),
        TripStatus = status,
        MaxCapacitySnapshot = 40,
        CapacityUsed = 0,
        RowVersion = new byte[8]
    };

    [Fact]
    public async Task ValidateStartAsync_ScheduledAndCorrectDriver_ReturnsNoNotifications()
    {
        await using var db = TestDbContextFactory.Create();
        var driverId = Guid.NewGuid();
        var trip = NewTrip(driverId, TripStatus.Scheduled);
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        var result = await new TripExecutionValidator(db).ValidateStartAsync(trip.Id, driverId);

        Assert.False(result.HasNotifications);
    }

    [Fact]
    public async Task ValidateStartAsync_WrongDriver_ReturnsForbidden()
    {
        await using var db = TestDbContextFactory.Create();
        var trip = NewTrip(Guid.NewGuid(), TripStatus.Scheduled);
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        var result = await new TripExecutionValidator(db).ValidateStartAsync(trip.Id, Guid.NewGuid());

        Assert.True(result.HasNotifications);
        Assert.Equal("Forbidden", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateStartAsync_AlreadyInProgress_ReturnsInvalidTransition()
    {
        await using var db = TestDbContextFactory.Create();
        var driverId = Guid.NewGuid();
        var trip = NewTrip(driverId, TripStatus.InProgress);
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        var result = await new TripExecutionValidator(db).ValidateStartAsync(trip.Id, driverId);

        Assert.True(result.HasNotifications);
        Assert.Equal("InvalidTransition", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateEndAsync_NotYetStarted_ReturnsInvalidTransition()
    {
        await using var db = TestDbContextFactory.Create();
        var driverId = Guid.NewGuid();
        var trip = NewTrip(driverId, TripStatus.Scheduled);
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        var result = await new TripExecutionValidator(db).ValidateEndAsync(trip.Id, driverId);

        Assert.True(result.HasNotifications);
        Assert.Equal("InvalidTransition", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateEndAsync_InProgressAndCorrectDriver_ReturnsNoNotifications()
    {
        await using var db = TestDbContextFactory.Create();
        var driverId = Guid.NewGuid();
        var trip = NewTrip(driverId, TripStatus.InProgress);
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        var result = await new TripExecutionValidator(db).ValidateEndAsync(trip.Id, driverId);

        Assert.False(result.HasNotifications);
    }
}

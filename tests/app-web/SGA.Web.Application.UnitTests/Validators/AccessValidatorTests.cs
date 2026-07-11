using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Web.Application.UnitTests.Fixtures;
using SGA.Web.Infrastructure.Persistence.Validators;

namespace SGA.Web.Application.UnitTests.Validators;

public class AccessValidatorTests
{
    private static Trip NewTrip(int capacityUsed, int maxCapacity) => new()
    {
        Id = Guid.NewGuid(),
        ScheduleId = Guid.NewGuid(),
        BusId = Guid.NewGuid(),
        DriverUserId = Guid.NewGuid(),
        TripDate = DateOnly.FromDateTime(DateTime.UtcNow),
        TripStatus = TripStatus.Scheduled,
        MaxCapacitySnapshot = maxCapacity,
        CapacityUsed = capacityUsed,
        RowVersion = new byte[8]
    };

    [Fact]
    public async Task ValidateAccessAsync_NoAuthorization_ReturnsUnauthorized()
    {
        await using var db = TestDbContextFactory.Create();
        var trip = NewTrip(capacityUsed: 0, maxCapacity: 40);
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        var result = await new AccessValidator(db).ValidateAccessAsync(Guid.NewGuid(), trip.Id);

        Assert.True(result.HasNotifications);
        Assert.Equal("Unauthorized", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateAccessAsync_ExpiredMonthlyTicket_ReturnsExpired()
    {
        await using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var trip = NewTrip(0, 40);
        db.Trips.Add(trip);
        db.Authorizations.Add(new Authorization
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AuthorizationType = AuthorizationType.MonthlyTicket,
            AuthorizationStatus = AuthorizationStatus.Active,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            IssuedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await new AccessValidator(db).ValidateAccessAsync(userId, trip.Id);

        Assert.True(result.HasNotifications);
        Assert.Equal("Expired", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateAccessAsync_CardWithNoBalance_ReturnsNoBalance()
    {
        await using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var trip = NewTrip(0, 40);
        db.Trips.Add(trip);
        db.Authorizations.Add(new Authorization
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AuthorizationType = AuthorizationType.RechargeableCard,
            AuthorizationStatus = AuthorizationStatus.Active,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Balance = 0,
            IssuedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await new AccessValidator(db).ValidateAccessAsync(userId, trip.Id);

        Assert.True(result.HasNotifications);
        Assert.Equal("NoBalance", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateAccessAsync_TripAtCapacity_ReturnsNoCapacity()
    {
        await using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var trip = NewTrip(capacityUsed: 40, maxCapacity: 40);
        db.Trips.Add(trip);
        db.Authorizations.Add(new Authorization
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AuthorizationType = AuthorizationType.RechargeableCard,
            AuthorizationStatus = AuthorizationStatus.Active,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Balance = 5,
            IssuedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await new AccessValidator(db).ValidateAccessAsync(userId, trip.Id);

        Assert.True(result.HasNotifications);
        Assert.Equal("NoCapacity", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateAccessAsync_AllConditionsMet_ReturnsNoNotifications()
    {
        await using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var trip = NewTrip(capacityUsed: 0, maxCapacity: 40);
        db.Trips.Add(trip);
        db.Authorizations.Add(new Authorization
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AuthorizationType = AuthorizationType.RechargeableCard,
            AuthorizationStatus = AuthorizationStatus.Active,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Balance = 5,
            IssuedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await new AccessValidator(db).ValidateAccessAsync(userId, trip.Id);

        Assert.False(result.HasNotifications);
    }
}

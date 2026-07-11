using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Desktop.Application.UnitTests.Fixtures;
using SGA.Desktop.Domain.Dtos;
using SGA.Desktop.Infrastructure.Persistence.Validators;

namespace SGA.Desktop.Application.UnitTests.Validators;

public class BusValidatorTests
{
    [Fact]
    public async Task ValidateForCreateAsync_DuplicatePlateNumber_ReturnsPlateNumberNotification()
    {
        await using var db = TestDbContextFactory.Create();
        db.Buses.Add(new Bus { Id = Guid.NewGuid(), PlateNumber = "DUP-001", Capacity = 30, BusStatus = BusStatus.Active });
        await db.SaveChangesAsync();

        var validator = new BusValidator(db);
        var result = await validator.ValidateForCreateAsync(new CreateBusDto { PlateNumber = "DUP-001", Capacity = 20 });

        Assert.True(result.HasNotifications);
        Assert.Equal("PlateNumber", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateForCreateAsync_ZeroCapacity_ReturnsCapacityNotification()
    {
        await using var db = TestDbContextFactory.Create();
        var validator = new BusValidator(db);

        var result = await validator.ValidateForCreateAsync(new CreateBusDto { PlateNumber = "NEW-001", Capacity = 0 });

        Assert.True(result.HasNotifications);
        Assert.Equal("Capacity", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateForCreateAsync_ValidBus_ReturnsNoNotifications()
    {
        await using var db = TestDbContextFactory.Create();
        var validator = new BusValidator(db);

        var result = await validator.ValidateForCreateAsync(new CreateBusDto { PlateNumber = "NEW-002", Capacity = 40 });

        Assert.False(result.HasNotifications);
    }
}

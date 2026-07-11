using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Desktop.Application.UnitTests.Fixtures;
using SGA.Desktop.Domain.Dtos;
using SGA.Desktop.Infrastructure.Persistence.Validators;

namespace SGA.Desktop.Application.UnitTests.Validators;

public class AuthorizationValidatorTests
{
    [Fact]
    public async Task ValidateForCreateAsync_MonthlyTicketWithEndDateBeforeStart_ReturnsEndDateNotification()
    {
        await using var db = TestDbContextFactory.Create();
        var validator = new AuthorizationValidator(db);

        var dto = new CreateAuthorizationDto
        {
            UserId = Guid.NewGuid(),
            AuthorizationType = AuthorizationType.MonthlyTicket,
            StartDate = new DateOnly(2026, 2, 1),
            EndDate = new DateOnly(2026, 1, 1)
        };

        var result = await validator.ValidateForCreateAsync(dto);

        Assert.True(result.HasNotifications);
        Assert.Equal("EndDate", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateForCreateAsync_RechargeableCardWithNegativeBalance_ReturnsBalanceNotification()
    {
        await using var db = TestDbContextFactory.Create();
        var validator = new AuthorizationValidator(db);

        var dto = new CreateAuthorizationDto
        {
            UserId = Guid.NewGuid(),
            AuthorizationType = AuthorizationType.RechargeableCard,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Balance = -5
        };

        var result = await validator.ValidateForCreateAsync(dto);

        Assert.True(result.HasNotifications);
        Assert.Equal("Balance", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateForUpdateAsync_CancelledAuthorization_IsImmutable()
    {
        await using var db = TestDbContextFactory.Create();
        var authorization = new Authorization
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AuthorizationType = AuthorizationType.RechargeableCard,
            AuthorizationStatus = AuthorizationStatus.Cancelled,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Balance = 0,
            IssuedAtUtc = DateTime.UtcNow
        };
        db.Authorizations.Add(authorization);
        await db.SaveChangesAsync();

        var validator = new AuthorizationValidator(db);
        var result = await validator.ValidateForUpdateAsync(authorization.Id, new UpdateAuthorizationDto
        {
            Balance = 10,
            AuthorizationStatus = AuthorizationStatus.Active
        });

        Assert.True(result.HasNotifications);
        Assert.Equal("Cancelled", result.Notifications.First().Key);
    }

    [Fact]
    public async Task ValidateForUpdateAsync_ActiveAuthorization_ReturnsNoNotifications()
    {
        await using var db = TestDbContextFactory.Create();
        var authorization = new Authorization
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AuthorizationType = AuthorizationType.RechargeableCard,
            AuthorizationStatus = AuthorizationStatus.Active,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Balance = 5,
            IssuedAtUtc = DateTime.UtcNow
        };
        db.Authorizations.Add(authorization);
        await db.SaveChangesAsync();

        var validator = new AuthorizationValidator(db);
        var result = await validator.ValidateForUpdateAsync(authorization.Id, new UpdateAuthorizationDto
        {
            Balance = 10,
            AuthorizationStatus = AuthorizationStatus.Active
        });

        Assert.False(result.HasNotifications);
    }
}

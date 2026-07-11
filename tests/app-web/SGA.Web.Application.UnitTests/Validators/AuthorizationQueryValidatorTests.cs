using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Web.Application.UnitTests.Fixtures;
using SGA.Web.Infrastructure.Persistence.Validators;

namespace SGA.Web.Application.UnitTests.Validators;

public class AuthorizationQueryValidatorTests
{
    [Fact]
    public async Task ValidateOwnershipAsync_OwnAuthorization_ReturnsNoNotifications()
    {
        await using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var authorization = new Authorization
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AuthorizationType = AuthorizationType.RechargeableCard,
            AuthorizationStatus = AuthorizationStatus.Active,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Balance = 5,
            IssuedAtUtc = DateTime.UtcNow
        };
        db.Authorizations.Add(authorization);
        await db.SaveChangesAsync();

        var result = await new AuthorizationQueryValidator(db).ValidateOwnershipAsync(userId, authorization.Id);

        Assert.False(result.HasNotifications);
    }

    [Fact]
    public async Task ValidateOwnershipAsync_AnotherUsersAuthorization_ReturnsForbidden()
    {
        await using var db = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var authorization = new Authorization
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            AuthorizationType = AuthorizationType.RechargeableCard,
            AuthorizationStatus = AuthorizationStatus.Active,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Balance = 5,
            IssuedAtUtc = DateTime.UtcNow
        };
        db.Authorizations.Add(authorization);
        await db.SaveChangesAsync();

        var result = await new AuthorizationQueryValidator(db).ValidateOwnershipAsync(Guid.NewGuid(), authorization.Id);

        Assert.True(result.HasNotifications);
        Assert.Equal("Forbidden", result.Notifications.First().Key);
    }
}

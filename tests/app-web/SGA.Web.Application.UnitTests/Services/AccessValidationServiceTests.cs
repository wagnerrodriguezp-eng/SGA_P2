using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Web.Application.Services;
using SGA.Web.Application.UnitTests.Fixtures;

namespace SGA.Web.Application.UnitTests.Services;

public class AccessValidationServiceTests
{
    private static (Trip trip, Authorization authorization) NewGrantableScenario() => (
        new Trip
        {
            Id = Guid.NewGuid(),
            TripStatus = TripStatus.Scheduled,
            MaxCapacitySnapshot = 1,
            CapacityUsed = 0,
            RowVersion = new byte[8]
        },
        new Authorization
        {
            Id = Guid.NewGuid(),
            AuthorizationType = AuthorizationType.RechargeableCard,
            AuthorizationStatus = AuthorizationStatus.Active,
            Balance = 5
        });

    [Fact]
    public async Task ValidateAndRecordAsync_ConcurrencyConflictOnFirstAttempt_RetriesAndSucceeds()
    {
        var (trip, authorization) = NewGrantableScenario();
        var tripRepository = new FakeTripRepository(trip, failuresBeforeSuccess: 1);
        var authorizationRepository = new FakeAuthorizationRepository(authorization);
        var usageRecordRepository = new FakeUsageRecordRepository();
        var auditWriter = new FakeAuditWriter();

        var service = new AccessValidationService(
            new FakeAccessValidator(), tripRepository, authorizationRepository, usageRecordRepository, auditWriter);

        var result = await service.ValidateAndRecordAsync(Guid.NewGuid(), trip.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(AccessResult.Granted, result.Data!.AccessResult);
        Assert.Equal(2, tripRepository.SaveChangesCallCount);
        Assert.Contains("Access.Granted", auditWriter.Actions);
    }

    [Fact]
    public async Task ValidateAndRecordAsync_ConcurrencyConflictOnAllAttempts_ReturnsConflict()
    {
        var (trip, authorization) = NewGrantableScenario();
        var tripRepository = new FakeTripRepository(trip, failuresBeforeSuccess: 3);
        var authorizationRepository = new FakeAuthorizationRepository(authorization);
        var usageRecordRepository = new FakeUsageRecordRepository();
        var auditWriter = new FakeAuditWriter();

        var service = new AccessValidationService(
            new FakeAccessValidator(), tripRepository, authorizationRepository, usageRecordRepository, auditWriter);

        var result = await service.ValidateAndRecordAsync(Guid.NewGuid(), trip.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResultStatus.Conflict, result.Status);
        Assert.Equal(3, tripRepository.SaveChangesCallCount);
    }
}

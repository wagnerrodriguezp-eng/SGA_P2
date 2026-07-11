using SGA.SharedKernel.Application.Results;
using SGA.SharedKernel.Domain.Entities;
using SGA.Web.Application.Persistence;
using SGA.Web.Domain.Validation;

namespace SGA.Web.Application.Services;

// Scoped to IAuthorizationQueryValidator.ValidateOwnershipAsync — the concrete mechanism behind
// "Students/Employees only see their own information."
public class AuthorizationQueryService
{
    private readonly IAuthorizationQueryValidator _ownershipValidator;
    private readonly IAuthorizationRepository _authorizationRepository;

    public AuthorizationQueryService(
        IAuthorizationQueryValidator ownershipValidator,
        IAuthorizationRepository authorizationRepository)
    {
        _ownershipValidator = ownershipValidator;
        _authorizationRepository = authorizationRepository;
    }

    public async Task<OperationResult<Authorization>> GetMineAsync(Guid userId, CancellationToken ct = default)
    {
        var authorization = await _authorizationRepository.GetActiveForUserAsync(userId, ct);
        if (authorization is null)
        {
            return OperationResult<Authorization>.Failure(
                OperationResultStatus.NotFound, "No active authorization found.");
        }

        var notifications = await _ownershipValidator.ValidateOwnershipAsync(userId, authorization.Id, ct);
        if (notifications.HasNotifications)
        {
            return OperationResult<Authorization>.Failure(
                OperationResultStatus.Forbidden, notifications.Notifications.Select(n => n.Message).ToArray());
        }

        return OperationResult<Authorization>.Success(authorization);
    }
}

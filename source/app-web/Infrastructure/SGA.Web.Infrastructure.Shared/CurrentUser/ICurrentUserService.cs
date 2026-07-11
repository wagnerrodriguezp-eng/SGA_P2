namespace SGA.Web.Infrastructure.Shared.CurrentUser;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    IReadOnlyCollection<string> Roles { get; }
}

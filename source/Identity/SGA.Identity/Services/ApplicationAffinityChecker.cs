namespace SGA.Identity.Services;

public class ApplicationAffinityChecker : IApplicationAffinityChecker
{
    private readonly HashSet<string> _allowedRoleNames;

    public ApplicationAffinityChecker(IEnumerable<string> allowedRoleNames)
    {
        _allowedRoleNames = new HashSet<string>(allowedRoleNames, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsAllowed(IEnumerable<string> userRoles) =>
        userRoles.Any(role => _allowedRoleNames.Contains(role));
}

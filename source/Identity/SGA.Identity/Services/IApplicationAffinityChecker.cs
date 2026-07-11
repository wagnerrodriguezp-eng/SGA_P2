namespace SGA.Identity.Services;

// Ensures only users native to a given application (allow-listed roles) may authenticate against
// it, even when the credential itself is otherwise valid. See shared-kernel/06 §5.
public interface IApplicationAffinityChecker
{
    bool IsAllowed(IEnumerable<string> userRoles);
}

using SGA.SharedKernel.Domain.Common;

namespace SGA.SharedKernel.Domain.Entities;

// Backs the desktop app's JWT refresh flow (app-desktop-wpf/09-api-design-and-contracts.md §4).
// Not used by app-web-mvc, whose shorter-lived browser session doesn't need one.
public class RefreshToken : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByToken { get; set; }
}

using SGA.SharedKernel.Domain.Common;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.SharedKernel.Domain.Entities;

public class NotificationMessage : BaseEntity<Guid>
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public MessageStatus MessageStatus { get; set; }

    // Capped at 3 — see messaging retry doc.
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptAtUtc { get; set; }
}

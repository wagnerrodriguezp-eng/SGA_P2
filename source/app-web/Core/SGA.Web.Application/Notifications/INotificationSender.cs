namespace SGA.Web.Application.Notifications;

// Intent-based facade over SGA.Shared.Messaging — implemented in Infrastructure.Shared, which
// selects the right template and delegates to NotificationDispatchService (send-now-fallback-to-outbox).
public interface INotificationSender
{
    Task SendAccountConfirmationAsync(string toEmail, string firstName, string confirmationLink, CancellationToken ct = default);

    Task SendPasswordResetAsync(string toEmail, string firstName, string resetLink, CancellationToken ct = default);

    Task SendIncidentNoticeAsync(string tripDescription, string incidentDescription, CancellationToken ct = default);
}

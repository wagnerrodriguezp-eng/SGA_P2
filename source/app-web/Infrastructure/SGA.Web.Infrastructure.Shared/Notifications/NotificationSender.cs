using Microsoft.Extensions.Options;
using SGA.Shared.Messaging.Abstractions;
using SGA.Shared.Messaging.Templates;
using SGA.Web.Application.Notifications;

namespace SGA.Web.Infrastructure.Shared.Notifications;

// Selects the right Messaging template and delegates to NotificationDispatchService
// (send-now-fallback-to-outbox) — keeps SGA.Web.Application decoupled from SGA.Shared.Messaging.
public class NotificationSender : INotificationSender
{
    private readonly INotificationDispatchService _dispatchService;
    private readonly IncidentNotificationSettings _incidentSettings;

    public NotificationSender(
        INotificationDispatchService dispatchService, IOptions<IncidentNotificationSettings> incidentSettings)
    {
        _dispatchService = dispatchService;
        _incidentSettings = incidentSettings.Value;
    }

    public Task SendAccountConfirmationAsync(
        string toEmail, string firstName, string confirmationLink, CancellationToken ct = default)
    {
        var (subject, body) = AccountConfirmationTemplate.Build(firstName, confirmationLink);
        return _dispatchService.DispatchAsync(toEmail, subject, body, ct);
    }

    public Task SendPasswordResetAsync(string toEmail, string firstName, string resetLink, CancellationToken ct = default)
    {
        var (subject, body) = PasswordResetTemplate.Build(firstName, resetLink);
        return _dispatchService.DispatchAsync(toEmail, subject, body, ct);
    }

    public Task SendIncidentNoticeAsync(string tripDescription, string incidentDescription, CancellationToken ct = default)
    {
        var (subject, body) = TripIncidentOrCancellationTemplate.BuildIncidentNotice(tripDescription, incidentDescription);
        return _dispatchService.DispatchAsync(_incidentSettings.RecipientEmail, subject, body, ct);
    }
}

using SGA.Shared.Messaging.Abstractions;
using SGA.Shared.Messaging.Templates;
using SGA.Desktop.Application.Notifications;

namespace SGA.Desktop.Infrastructure.Shared.Notifications;

public class NotificationSender : INotificationSender
{
    private readonly INotificationDispatchService _dispatchService;

    public NotificationSender(INotificationDispatchService dispatchService)
    {
        _dispatchService = dispatchService;
    }

    public Task SendAccountConfirmationAsync(
        string toEmail, string firstName, string confirmationLink, CancellationToken ct = default)
    {
        var (subject, body) = AccountConfirmationTemplate.Build(firstName, confirmationLink);
        return _dispatchService.DispatchAsync(toEmail, subject, body, ct);
    }

    public Task SendTripCancellationAsync(string toEmail, string tripDescription, string reason, CancellationToken ct = default)
    {
        // Per-recipient first names aren't readily available for this fan-out without extra
        // Identity round trips, so the template uses a generic greeting here.
        var (subject, body) = TripIncidentOrCancellationTemplate.BuildCancellation("there", tripDescription, reason);
        return _dispatchService.DispatchAsync(toEmail, subject, body, ct);
    }
}

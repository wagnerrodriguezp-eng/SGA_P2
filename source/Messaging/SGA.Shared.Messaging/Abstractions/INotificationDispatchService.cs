namespace SGA.Shared.Messaging.Abstractions;

// Callers never call IEmailSender directly — DispatchAsync is awaited but a transient send
// failure never propagates as an exception; it degrades to the NotificationMessage outbox instead.
public interface INotificationDispatchService
{
    Task DispatchAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}

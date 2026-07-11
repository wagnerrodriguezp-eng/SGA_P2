namespace SGA.Desktop.Application.Notifications;

public interface INotificationSender
{
    Task SendAccountConfirmationAsync(string toEmail, string firstName, string confirmationLink, CancellationToken ct = default);

    Task SendTripCancellationAsync(string toEmail, string tripDescription, string reason, CancellationToken ct = default);
}

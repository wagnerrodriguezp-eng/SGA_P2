namespace SGA.SharedKernel.Domain.Notifications;

public class NotificationContext
{
    private readonly List<Notification> _notifications = new();

    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();
    public bool HasNotifications => _notifications.Count > 0;

    public void AddNotification(string key, string message) =>
        _notifications.Add(new Notification(key, message));
}

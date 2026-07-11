using Microsoft.Extensions.Logging;
using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Shared.Messaging.Abstractions;

namespace SGA.Shared.Messaging;

public class NotificationDispatchService : INotificationDispatchService
{
    private readonly IEmailSender _emailSender;
    private readonly IGenericRepository<NotificationMessage, Guid> _outbox;
    private readonly ILogger<NotificationDispatchService> _logger;

    public NotificationDispatchService(
        IEmailSender emailSender,
        IGenericRepository<NotificationMessage, Guid> outbox,
        ILogger<NotificationDispatchService> logger)
    {
        _emailSender = emailSender;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task DispatchAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        try
        {
            await _emailSender.SendAsync(toEmail, subject, htmlBody, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Email send failed for {Recipient}; queuing to the outbox", toEmail);

            var message = new NotificationMessage
            {
                Id = Guid.NewGuid(),
                RecipientEmail = toEmail,
                Subject = subject,
                Body = htmlBody,
                MessageStatus = MessageStatus.Pending,
                AttemptCount = 1,
                LastAttemptAtUtc = DateTime.UtcNow
            };

            await _outbox.AddAsync(message, ct);
            await _outbox.SaveChangesAsync(ct);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Shared.Messaging.Abstractions;

namespace SGA.Shared.Messaging;

public class NotificationRetryBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private const int MaxAttempts = 3;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationRetryBackgroundService> _logger;

    public NotificationRetryBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationRetryBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RetryPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification retry pass failed");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // shutting down
            }
        }
    }

    private async Task RetryPendingMessagesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IGenericRepository<NotificationMessage, Guid>>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var candidates = (await outbox.GetAllAsync(includeInactive: true, ct))
            .Where(m => m.MessageStatus is MessageStatus.Pending or MessageStatus.Failed && m.AttemptCount < MaxAttempts)
            .ToList();

        foreach (var message in candidates)
        {
            try
            {
                await emailSender.SendAsync(message.RecipientEmail, message.Subject, message.Body, ct);
                message.MessageStatus = MessageStatus.Sent;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Retry send failed for outbox message {MessageId}", message.Id);
                message.AttemptCount++;
                message.LastAttemptAtUtc = DateTime.UtcNow;
                message.MessageStatus = message.AttemptCount >= MaxAttempts ? MessageStatus.Abandoned : MessageStatus.Failed;
            }

            await outbox.UpdateAsync(message, ct);
        }

        if (candidates.Count > 0)
        {
            await outbox.SaveChangesAsync(ct);
        }
    }
}

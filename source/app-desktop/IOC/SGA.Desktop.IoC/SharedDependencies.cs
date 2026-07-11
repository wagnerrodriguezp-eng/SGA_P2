using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGA.SharedKernel.Domain.Settings;
using SGA.Shared.Messaging;
using SGA.Shared.Messaging.Abstractions;
using SGA.Desktop.Application.Notifications;
using SGA.Desktop.Infrastructure.Shared.Notifications;

namespace SGA.Desktop.IoC;

public static class SharedDependencies
{
    public static IServiceCollection AddSharedDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        services.AddScoped<IEmailSender, MailKitEmailSender>();
        services.AddScoped<INotificationDispatchService, NotificationDispatchService>();
        services.AddScoped<INotificationSender, NotificationSender>();
        services.AddHostedService<NotificationRetryBackgroundService>();

        return services;
    }
}

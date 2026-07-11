using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGA.SharedKernel.Domain.Settings;
using SGA.Shared.Messaging;
using SGA.Shared.Messaging.Abstractions;
using SGA.Web.Application.Notifications;
using SGA.Web.Infrastructure.Shared.Notifications;

namespace SGA.Web.IoC;

public static class SharedDependencies
{
    public static IServiceCollection AddSharedDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<IncidentNotificationSettings>(configuration.GetSection("IncidentNotifications"));

        services.AddScoped<IEmailSender, MailKitEmailSender>();
        services.AddScoped<INotificationDispatchService, NotificationDispatchService>();
        services.AddScoped<INotificationSender, NotificationSender>();
        services.AddHostedService<NotificationRetryBackgroundService>();

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using SGA.Desktop.Application.Auditing;
using SGA.Desktop.Infrastructure.Shared.Auditing;
using SGA.Desktop.Infrastructure.Shared.CurrentUser;

namespace SGA.Desktop.IoC;

public static class InfrastructureDependencies
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
    {
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}

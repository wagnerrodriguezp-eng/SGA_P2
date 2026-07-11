using Microsoft.Extensions.DependencyInjection;
using SGA.Web.Application.Auditing;
using SGA.Web.Infrastructure.Shared.Auditing;
using SGA.Web.Infrastructure.Shared.CurrentUser;

namespace SGA.Web.IoC;

public static class InfrastructureDependencies
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
    {
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}

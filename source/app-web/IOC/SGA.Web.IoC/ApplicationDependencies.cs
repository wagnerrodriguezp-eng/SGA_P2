using Microsoft.Extensions.DependencyInjection;
using SGA.Web.Application.Services;

namespace SGA.Web.IoC;

public static class ApplicationDependencies
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddScoped<AccessValidationService>();
        services.AddScoped<ScheduleQueryService>();
        services.AddScoped<AuthorizationQueryService>();
        services.AddScoped<TripExecutionService>();
        services.AddScoped<AccountService>();
        return services;
    }
}

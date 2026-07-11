using Microsoft.Extensions.DependencyInjection;
using SGA.Desktop.Application.Services;

namespace SGA.Desktop.IoC;

public static class ApplicationDependencies
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddScoped<FleetManagementService>();
        services.AddScoped<DriverManagementService>();
        services.AddScoped<RouteSchedulingService>();
        services.AddScoped<TripAssignmentService>();
        services.AddScoped<AuthorizationManagementService>();
        services.AddScoped<UserManagementService>();
        services.AddScoped<ReportingService>();
        services.AddScoped<AuditQueryService>();
        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using SGA.Desktop.Domain.Validation;
using SGA.Desktop.Infrastructure.Persistence.Validators;

namespace SGA.Desktop.IoC;

public static class DomainDependencies
{
    public static IServiceCollection AddDomainDependencies(this IServiceCollection services)
    {
        services.AddScoped<IBusValidator, BusValidator>();
        services.AddScoped<IDriverValidator, DriverValidator>();
        services.AddScoped<IRouteScheduleValidator, RouteScheduleValidator>();
        services.AddScoped<ITripAssignmentValidator, TripAssignmentValidator>();
        services.AddScoped<IAuthorizationValidator, AuthorizationValidator>();
        return services;
    }
}

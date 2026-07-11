using Microsoft.Extensions.DependencyInjection;
using SGA.Web.Domain.Validation;
using SGA.Web.Infrastructure.Persistence.Validators;

namespace SGA.Web.IoC;

public static class DomainDependencies
{
    public static IServiceCollection AddDomainDependencies(this IServiceCollection services)
    {
        services.AddScoped<IAccessValidator, AccessValidator>();
        services.AddScoped<ITripExecutionValidator, TripExecutionValidator>();
        services.AddScoped<IAuthorizationQueryValidator, AuthorizationQueryValidator>();
        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SGA.Web.IoC;

public static class WebIoCServiceCollectionExtensions
{
    public static IServiceCollection AddWebApplicationServices(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddDomainDependencies()
            .AddPersistenceDependencies(configuration)
            .AddApplicationDependencies()
            .AddInfrastructureDependencies()
            .AddSharedDependencies(configuration);
}

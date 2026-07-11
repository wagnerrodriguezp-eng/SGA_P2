using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SGA.Desktop.IoC;

public static class DesktopIoCServiceCollectionExtensions
{
    public static IServiceCollection AddDesktopApplicationServices(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddDomainDependencies()
            .AddPersistenceDependencies(configuration)
            .AddApplicationDependencies()
            .AddInfrastructureDependencies()
            .AddSharedDependencies(configuration);
}

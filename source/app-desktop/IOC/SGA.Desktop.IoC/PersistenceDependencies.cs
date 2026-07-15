using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGA.SharedKernel.Application.Persistence;
using SGA.Desktop.Application.Identity;
using SGA.Desktop.Application.Persistence;
using SGA.Desktop.Infrastructure.Persistence;
using SGA.Desktop.Infrastructure.Persistence.Identity;
using SGA.Desktop.Infrastructure.Persistence.Repositories;

namespace SGA.Desktop.IoC;

public static class PersistenceDependencies
{
    public static IServiceCollection AddPersistenceDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();

        services.AddDbContext<DesktopAppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)));

        services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
        services.AddScoped<IBusRepository, BusRepository>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IReportingRepository, ReportingRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IIdentityGateway, IdentityGatewayAdapter>();

        return services;
    }
}

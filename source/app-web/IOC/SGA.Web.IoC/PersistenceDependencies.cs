using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGA.SharedKernel.Application.Persistence;
using SGA.Web.Application.Identity;
using SGA.Web.Application.Persistence;
using SGA.Web.Infrastructure.Persistence;
using SGA.Web.Infrastructure.Persistence.Identity;
using SGA.Web.Infrastructure.Persistence.Repositories;

namespace SGA.Web.IoC;

public static class PersistenceDependencies
{
    public static IServiceCollection AddPersistenceDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();

        services.AddDbContext<WebAppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)));

        services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
        services.AddScoped<IIdentityGateway, IdentityGatewayAdapter>();

        return services;
    }
}

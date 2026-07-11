using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SGA.Identity.Constants;
using SGA.Identity.Entities;
using SGA.Identity.Services;
using SGA.SharedKernel.Domain.Settings;

namespace SGA.Identity.DependencyInjection;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        IEnumerable<string> allowedRoleNames)
        where TDbContext : DbContext
    {
        var roleNames = allowedRoleNames.ToArray();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<TDbContext>()
            .AddSignInManager<SignInManager<ApplicationUser>>()
            .AddDefaultTokenProviders();

        services.AddSingleton<IApplicationAffinityChecker>(_ => new ApplicationAffinityChecker(roleNames));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAccountService, AccountService>();

        var jwt = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        string.IsNullOrEmpty(jwt.SigningKey) ? new string('0', 64) : jwt.SigningKey))
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("StudentOrEmployee", p => p.RequireRole(RoleNames.Student, RoleNames.Employee))
            .AddPolicy("DriverOnly", p => p.RequireRole(RoleNames.Driver))
            .AddPolicy("TransportAdminOnly", p => p.RequireRole(RoleNames.TransportAdmin));

        return services;
    }
}

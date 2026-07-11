using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;
using SGA.Identity.Constants;
using SGA.Identity.DependencyInjection;
using SGA.SharedKernel.Application.Results;
using SGA.Desktop.Api;
using SGA.Desktop.Api.HealthChecks;
using SGA.Desktop.Infrastructure.Persistence;
using SGA.Desktop.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day));

builder.Services.AddControllers();

// Keeps the model-binding validation error shape consistent with OperationResult everywhere else.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(
        OperationResult.Failure(
            OperationResultStatus.ValidationError,
            context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray()));
});

// Program.cs stays free of Identity plumbing beyond this single call, per app-desktop-wpf/06 §1.
builder.Services.AddIdentityInfrastructure<DesktopAppDbContext>(
    builder.Configuration,
    allowedRoleNames: new[] { RoleNames.TransportAdmin });

builder.Services.AddDesktopApplicationServices(builder.Configuration);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddMvc();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();

// Startup resilience — Polly retry (3x, exponential backoff 2s/4s/8s) around the initial DB
// connectivity check. If all attempts fail, the app still finishes starting so /health can report
// Unhealthy rather than the process refusing to bind at all.
var startupRetryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

await startupRetryPolicy.ExecuteAsync(async () =>
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<DesktopAppDbContext>();
    await db.Database.CanConnectAsync();
});

await TransportAdminSeeder.SeedAsync(app.Services, app.Configuration, app.Services.GetRequiredService<ILogger<Program>>());

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(feature?.Error, "Unhandled exception");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(OperationResult.Failure(
            OperationResultStatus.UnexpectedError, "An unexpected error occurred. Please try again later."));
    });
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

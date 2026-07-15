using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Polly;
using Serilog;
using SGA.Identity.Constants;
using SGA.Identity.DependencyInjection;
using SGA.SharedKernel.Application.Results;
using SGA.Web.Api.HealthChecks;
using SGA.Web.Infrastructure.Persistence;
using SGA.Web.IoC;

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

// Program.cs stays free of Identity plumbing beyond this single call, per shared-kernel/06 §6.
builder.Services.AddIdentityInfrastructure<WebAppDbContext>(
    builder.Configuration,
    allowedRoleNames: new[] { RoleNames.Student, RoleNames.Employee, RoleNames.Driver });

builder.Services.AddWebApplicationServices(builder.Configuration);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddMvc();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SGA.Web.Api", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the access token returned by POST /api/v1/auth/login."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

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
    var db = scope.ServiceProvider.GetRequiredService<WebAppDbContext>();
    // CanConnectAsync() swallows connection failures and returns false rather than throwing —
    // without this check, Policy.Handle<Exception>() never sees a failure to retry on.
    if (!await db.Database.CanConnectAsync())
    {
        throw new InvalidOperationException("Database is not reachable yet.");
    }
});

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

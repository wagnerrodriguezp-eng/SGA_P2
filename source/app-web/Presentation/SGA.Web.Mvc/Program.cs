using Microsoft.AspNetCore.Authentication.Cookies;
using Polly;
using SGA.Web.Mvc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

// MVC controllers call the API through this typed HttpClient only — Polly retry + circuit breaker
// keep a fully-down API from making every subsequent request hang on repeated timeouts.
builder.Services.AddHttpClient<ISgaApiClient, SgaApiClient>(client =>
    {
        var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
            ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");
        client.BaseAddress = new Uri(apiBaseUrl);
    })
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(attempt)))
    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

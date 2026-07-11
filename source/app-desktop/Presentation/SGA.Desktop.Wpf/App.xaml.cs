using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using SGA.Desktop.Wpf.Services;
using SGA.Desktop.Wpf.ViewModels;
using SGA.Desktop.Wpf.Views;

namespace SGA.Desktop.Wpf;

public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // The system must never crash from an unhandled exception on the UI thread or a
        // fire-and-forget task — per app-desktop-wpf/08-resilience-and-error-handling.md §2.2.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<ICurrentUserService, CurrentUserService>();
                services.AddSingleton<INavigationService, NavigationService>();

                services.AddHttpClient<ISgaDesktopApiClient, SgaDesktopApiClient>(client =>
                    {
                        var apiBaseUrl = context.Configuration["Api:BaseUrl"]
                            ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");
                        client.BaseAddress = new Uri(apiBaseUrl);
                    })
                    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(attempt)))
                    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

                services.AddTransient<LoginViewModel>();
                services.AddTransient<ShellViewModel>();
                services.AddTransient<BusesViewModel>();
                services.AddTransient<DriversViewModel>();
                services.AddTransient<RoutesViewModel>();
                services.AddTransient<TripAssignmentViewModel>();
                services.AddTransient<TripMonitorViewModel>();
                services.AddTransient<AuthorizationsViewModel>();
                services.AddTransient<UsersViewModel>();
                services.AddTransient<ReportsViewModel>();
                services.AddTransient<AuditLogViewModel>();

                services.AddTransient<LoginView>();
                services.AddTransient<ShellView>();
                services.AddTransient<BusesView>();
                services.AddTransient<DriversView>();
                services.AddTransient<RoutesView>();
                services.AddTransient<TripAssignmentView>();
                services.AddTransient<TripMonitorView>();
                services.AddTransient<AuthorizationsView>();
                services.AddTransient<UsersView>();
                services.AddTransient<ReportsView>();
                services.AddTransient<AuditLogView>();
            })
            .Build();

        _host.Start();
        ShowLogin();
    }

    private void ShowLogin()
    {
        var loginView = _host!.Services.GetRequiredService<LoginView>();
        var loginViewModel = _host.Services.GetRequiredService<LoginViewModel>();
        loginViewModel.LoginSucceeded += (_, _) => ShowShell();
        loginView.DataContext = loginViewModel;

        var previousWindow = Current.MainWindow;
        Current.MainWindow = loginView;
        loginView.Show();
        previousWindow?.Close();
    }

    private void ShowShell()
    {
        var shellView = _host!.Services.GetRequiredService<ShellView>();
        var shellViewModel = _host.Services.GetRequiredService<ShellViewModel>();
        shellViewModel.LoggedOut += (_, _) => ShowLogin();
        shellView.DataContext = shellViewModel;

        var previousWindow = Current.MainWindow;
        Current.MainWindow = shellView;
        shellView.Show();
        previousWindow?.Close();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.StopAsync().GetAwaiter().GetResult();
        _host?.Dispose();
        base.OnExit(e);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
    {
        MessageBox.Show(
            "Something went wrong, but the application will keep running. If this keeps happening, contact support.",
            "SGA-ITLA", MessageBoxButton.OK, MessageBoxImage.Warning);
        args.Handled = true;
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args)
    {
        args.SetObserved();
    }
}

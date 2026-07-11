using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SGA.Desktop.Wpf.ViewModels;
using SGA.Desktop.Wpf.Views;

namespace SGA.Desktop.Wpf.Services;

// Swaps the shell's content region between Views resolved from DI — Views never `new` their
// ViewModel directly, and ViewModels never reference a View type.
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Type> _viewModelToView = new();

    public object? CurrentView { get; private set; }
    public event Action? CurrentViewChanged;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        Register<BusesViewModel, BusesView>();
        Register<DriversViewModel, DriversView>();
        Register<RoutesViewModel, RoutesView>();
        Register<TripAssignmentViewModel, TripAssignmentView>();
        Register<TripMonitorViewModel, TripMonitorView>();
        Register<AuthorizationsViewModel, AuthorizationsView>();
        Register<UsersViewModel, UsersView>();
        Register<ReportsViewModel, ReportsView>();
        Register<AuditLogViewModel, AuditLogView>();
    }

    private void Register<TViewModel, TView>()
        where TViewModel : class
        where TView : class
        => _viewModelToView[typeof(TViewModel)] = typeof(TView);

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        var viewType = _viewModelToView[typeof(TViewModel)];
        var view = (FrameworkElement)_serviceProvider.GetRequiredService(viewType);
        view.DataContext = viewModel;

        CurrentView = view;
        CurrentViewChanged?.Invoke();
    }
}

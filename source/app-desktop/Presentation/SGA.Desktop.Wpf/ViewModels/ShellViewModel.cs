using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGA.Desktop.Wpf.Services;

namespace SGA.Desktop.Wpf.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    public ICurrentUserService CurrentUserService { get; }

    [ObservableProperty]
    private object? currentView;

    public event EventHandler? LoggedOut;

    public ShellViewModel(INavigationService navigationService, ICurrentUserService currentUserService)
    {
        _navigationService = navigationService;
        CurrentUserService = currentUserService;
        _navigationService.CurrentViewChanged += () => CurrentView = _navigationService.CurrentView;

        NavigateTo<BusesViewModel>();
    }

    [RelayCommand]
    private void NavigateBuses() => NavigateTo<BusesViewModel>();

    [RelayCommand]
    private void NavigateDrivers() => NavigateTo<DriversViewModel>();

    [RelayCommand]
    private void NavigateRoutes() => NavigateTo<RoutesViewModel>();

    [RelayCommand]
    private void NavigateTripAssignment() => NavigateTo<TripAssignmentViewModel>();

    [RelayCommand]
    private void NavigateTripMonitor() => NavigateTo<TripMonitorViewModel>();

    [RelayCommand]
    private void NavigateAuthorizations() => NavigateTo<AuthorizationsViewModel>();

    [RelayCommand]
    private void NavigateUsers() => NavigateTo<UsersViewModel>();

    [RelayCommand]
    private void NavigateReports() => NavigateTo<ReportsViewModel>();

    [RelayCommand]
    private void NavigateAuditLog() => NavigateTo<AuditLogViewModel>();

    [RelayCommand]
    private void Logout()
    {
        CurrentUserService.ClearSession();
        LoggedOut?.Invoke(this, EventArgs.Empty);
    }

    private void NavigateTo<TViewModel>() where TViewModel : class => _navigationService.NavigateTo<TViewModel>();
}

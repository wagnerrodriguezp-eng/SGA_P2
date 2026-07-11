using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGA.Desktop.Wpf.Models;
using SGA.Desktop.Wpf.Services;

namespace SGA.Desktop.Wpf.ViewModels;

public partial class RoutesViewModel : ObservableObject
{
    private readonly ISgaDesktopApiClient _apiClient;

    [ObservableProperty] private ObservableCollection<RouteModel> routes = new();
    [ObservableProperty] private RouteModel? selectedRoute;
    [ObservableProperty] private ObservableCollection<StopModel> stops = new();
    [ObservableProperty] private ObservableCollection<ScheduleModel> schedules = new();
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private string? statusMessage;

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string? description;
    [ObservableProperty] private RouteStatusOption routeStatus = RouteStatusOption.Active;

    [ObservableProperty] private string newStopName = string.Empty;
    [ObservableProperty] private int newStopOrder = 1;

    [ObservableProperty] private TimeSpan newScheduleDepartureTime = new(8, 0, 0);
    [ObservableProperty] private int newScheduleDaysOfWeekMask = 31; // Mon-Fri by default

    public IReadOnlyList<RouteStatusOption> RouteStatusOptions { get; } = Enum.GetValues<RouteStatusOption>();

    public RoutesViewModel(ISgaDesktopApiClient apiClient)
    {
        _apiClient = apiClient;
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _apiClient.GetAsync<List<RouteModel>>("api/v1/routes");
            if (result is null)
            {
                StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
                return;
            }
            Routes = new ObservableCollection<RouteModel>(result.Data ?? new List<RouteModel>());
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void New()
    {
        SelectedRoute = null;
        IsEditing = true;
        Name = string.Empty;
        Description = null;
        RouteStatus = RouteStatusOption.Active;
        Stops.Clear();
        Schedules.Clear();
    }

    [RelayCommand]
    private async Task SelectAsync(RouteModel? route)
    {
        if (route is null)
        {
            return;
        }
        SelectedRoute = route;
        IsEditing = true;
        Name = route.Name;
        Description = route.Description;
        RouteStatus = route.RouteStatus;

        IsBusy = true;
        try
        {
            var stopsResult = await _apiClient.GetAsync<List<StopModel>>($"api/v1/routes/{route.Id}/stops");
            Stops = new ObservableCollection<StopModel>(stopsResult?.Data ?? new List<StopModel>());

            var schedulesResult = await _apiClient.GetAsync<List<ScheduleModel>>($"api/v1/routes/{route.Id}/schedules");
            Schedules = new ObservableCollection<ScheduleModel>(schedulesResult?.Data ?? new List<ScheduleModel>());
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelEdit() => IsEditing = false;

    [RelayCommand]
    private async Task SaveAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            if (SelectedRoute is null)
            {
                var result = await _apiClient.PostAsync<RouteModel>("api/v1/routes", new { name = Name, description = Description });
                if (result is null || !result.IsSuccess)
                {
                    StatusMessage = result?.Message ?? "The service is temporarily unavailable. Please try again shortly.";
                    return;
                }
                SelectedRoute = result.Data;
            }
            else
            {
                var result = await _apiClient.PutAsync<RouteModel>($"api/v1/routes/{SelectedRoute.Id}", new
                {
                    name = Name,
                    description = Description,
                    routeStatus = (int)RouteStatus
                });
                if (result is null || !result.IsSuccess)
                {
                    StatusMessage = result?.Message ?? "The service is temporarily unavailable. Please try again shortly.";
                    return;
                }
            }

            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddStopAsync()
    {
        if (SelectedRoute is null || string.IsNullOrWhiteSpace(NewStopName))
        {
            return;
        }
        IsBusy = true;
        try
        {
            var result = await _apiClient.PostAsync<StopModel>($"api/v1/routes/{SelectedRoute.Id}/stops", new
            {
                routeId = SelectedRoute.Id,
                name = NewStopName,
                order = NewStopOrder
            });
            if (result is { IsSuccess: true, Data: not null })
            {
                Stops.Add(result.Data);
                NewStopName = string.Empty;
            }
            else
            {
                StatusMessage = result?.Message ?? "Unable to add stop.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddScheduleAsync()
    {
        if (SelectedRoute is null)
        {
            return;
        }
        IsBusy = true;
        try
        {
            var result = await _apiClient.PostAsync<ScheduleModel>($"api/v1/routes/{SelectedRoute.Id}/schedules", new
            {
                routeId = SelectedRoute.Id,
                departureTime = NewScheduleDepartureTime,
                daysOfWeekMask = NewScheduleDaysOfWeekMask
            });
            if (result is { IsSuccess: true, Data: not null })
            {
                Schedules.Add(result.Data);
            }
            else
            {
                StatusMessage = result?.Message ?? "Unable to add schedule.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}

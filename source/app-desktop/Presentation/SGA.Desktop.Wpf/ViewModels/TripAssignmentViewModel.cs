using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGA.Desktop.Wpf.Models;
using SGA.Desktop.Wpf.Services;

namespace SGA.Desktop.Wpf.ViewModels;

public partial class TripAssignmentViewModel : ObservableObject
{
    private readonly ISgaDesktopApiClient _apiClient;

    [ObservableProperty] private ObservableCollection<BusModel> buses = new();
    [ObservableProperty] private ObservableCollection<DriverProfileModel> drivers = new();
    [ObservableProperty] private ObservableCollection<RouteModel> routes = new();
    [ObservableProperty] private ObservableCollection<ScheduleModel> schedules = new();

    [ObservableProperty] private RouteModel? selectedRoute;
    [ObservableProperty] private ScheduleModel? selectedSchedule;
    [ObservableProperty] private BusModel? selectedBus;
    [ObservableProperty] private DriverProfileModel? selectedDriver;
    [ObservableProperty] private DateTime tripDate = DateTime.Today;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? statusMessage;

    public TripAssignmentViewModel(ISgaDesktopApiClient apiClient)
    {
        _apiClient = apiClient;
        _ = LoadReferenceDataAsync();
    }

    [RelayCommand]
    private async Task LoadReferenceDataAsync()
    {
        IsBusy = true;
        try
        {
            var busesResult = await _apiClient.GetAsync<List<BusModel>>("api/v1/buses");
            Buses = new ObservableCollection<BusModel>(busesResult?.Data ?? new List<BusModel>());

            var driversResult = await _apiClient.GetAsync<List<DriverProfileModel>>("api/v1/drivers");
            Drivers = new ObservableCollection<DriverProfileModel>(driversResult?.Data ?? new List<DriverProfileModel>());

            var routesResult = await _apiClient.GetAsync<List<RouteModel>>("api/v1/routes");
            Routes = new ObservableCollection<RouteModel>(routesResult?.Data ?? new List<RouteModel>());
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedRouteChanged(RouteModel? value)
    {
        _ = LoadSchedulesAsync(value);
    }

    private async Task LoadSchedulesAsync(RouteModel? route)
    {
        if (route is null)
        {
            Schedules = new ObservableCollection<ScheduleModel>();
            return;
        }

        var result = await _apiClient.GetAsync<List<ScheduleModel>>($"api/v1/routes/{route.Id}/schedules");
        Schedules = new ObservableCollection<ScheduleModel>(result?.Data ?? new List<ScheduleModel>());
    }

    [RelayCommand]
    private async Task AssignAsync()
    {
        if (SelectedSchedule is null || SelectedBus is null || SelectedDriver is null)
        {
            StatusMessage = "Please select a schedule, bus, and driver.";
            return;
        }

        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _apiClient.PostAsync<TripModel>("api/v1/trips", new
            {
                scheduleId = SelectedSchedule.Id,
                busId = SelectedBus.Id,
                driverUserId = SelectedDriver.UserId,
                tripDate = DateOnly.FromDateTime(TripDate)
            });

            if (result is null)
            {
                StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
                return;
            }

            StatusMessage = result.IsSuccess
                ? "Trip assigned successfully."
                : (result.Errors.Count > 0 ? string.Join("; ", result.Errors) : result.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

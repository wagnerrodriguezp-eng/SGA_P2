using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGA.Desktop.Wpf.Models;
using SGA.Desktop.Wpf.Services;

namespace SGA.Desktop.Wpf.ViewModels;

// A "live-ish" status board — polls GET /api/trips on an interval, per
// app-desktop-wpf/10-presentation-wpf-mvvm.md §4.
public partial class TripMonitorViewModel : ObservableObject, IDisposable
{
    private readonly ISgaDesktopApiClient _apiClient;
    private readonly DispatcherTimer _timer;

    [ObservableProperty] private ObservableCollection<TripModel> trips = new();
    [ObservableProperty] private DateTime fromDate = DateTime.Today;
    [ObservableProperty] private DateTime toDate = DateTime.Today.AddDays(7);
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private string cancelReason = string.Empty;

    public TripMonitorViewModel(ISgaDesktopApiClient apiClient)
    {
        _apiClient = apiClient;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _timer.Tick += async (_, _) => await LoadAsync();
        _timer.Start();
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _apiClient.GetAsync<List<TripModel>>(
                $"api/v1/trips?from={DateOnly.FromDateTime(FromDate):yyyy-MM-dd}&to={DateOnly.FromDateTime(ToDate):yyyy-MM-dd}");
            if (result is null)
            {
                StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
                return;
            }
            Trips = new ObservableCollection<TripModel>(result.Data ?? new List<TripModel>());
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelTripAsync(TripModel? trip)
    {
        if (trip is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _apiClient.PostAsync<TripModel>($"api/v1/trips/{trip.Id}/cancel", new
            {
                tripId = trip.Id,
                reason = string.IsNullOrWhiteSpace(CancelReason) ? "Cancelled by administrator." : CancelReason
            });

            if (result is { IsSuccess: true })
            {
                await LoadAsync();
            }
            else
            {
                StatusMessage = result?.Message ?? "Unable to cancel this trip.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Dispose() => _timer.Stop();
}

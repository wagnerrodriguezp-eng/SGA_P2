using System.Collections.ObjectModel;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGA.Desktop.Wpf.Models;
using SGA.Desktop.Wpf.Services;

namespace SGA.Desktop.Wpf.ViewModels;

public partial class BusesViewModel : ObservableObject
{
    private readonly ISgaDesktopApiClient _apiClient;

    [ObservableProperty] private ObservableCollection<BusModel> buses = new();
    [ObservableProperty] private BusModel? selectedBus;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private string? statusMessage;

    [ObservableProperty] private string plateNumber = string.Empty;
    [ObservableProperty] private string? modelName;
    [ObservableProperty] private int? year;
    [ObservableProperty] private int capacity = 1;
    [ObservableProperty] private BusStatusOption busStatus = BusStatusOption.Active;

    public IReadOnlyList<BusStatusOption> BusStatusOptions { get; } = Enum.GetValues<BusStatusOption>();

    public BusesViewModel(ISgaDesktopApiClient apiClient)
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
            var result = await _apiClient.GetAsync<List<BusModel>>("api/v1/buses");
            if (result is null)
            {
                StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
                return;
            }
            Buses = new ObservableCollection<BusModel>(result.Data ?? new List<BusModel>());
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void New()
    {
        SelectedBus = null;
        IsEditing = true;
        PlateNumber = string.Empty;
        ModelName = null;
        Year = null;
        Capacity = 1;
        BusStatus = BusStatusOption.Active;
    }

    [RelayCommand]
    private void Edit(BusModel? bus)
    {
        if (bus is null)
        {
            return;
        }
        SelectedBus = bus;
        IsEditing = true;
        PlateNumber = bus.PlateNumber;
        ModelName = bus.Model;
        Year = bus.Year;
        Capacity = bus.Capacity;
        BusStatus = bus.BusStatus;
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
            if (SelectedBus is null)
            {
                var result = await _apiClient.PostAsync<BusModel>("api/v1/buses", new
                {
                    plateNumber = PlateNumber,
                    model = ModelName,
                    year = Year,
                    capacity = Capacity
                });
                if (!await HandleWriteResultAsync(result))
                {
                    return;
                }
            }
            else
            {
                var result = await _apiClient.PutAsync<BusModel>($"api/v1/buses/{SelectedBus.Id}", new
                {
                    model = ModelName,
                    year = Year,
                    capacity = Capacity,
                    busStatus = (int)BusStatus
                });
                if (!await HandleWriteResultAsync(result))
                {
                    return;
                }
            }

            IsEditing = false;
            await LoadAsync();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeactivateAsync(BusModel? bus)
    {
        if (bus is null)
        {
            return;
        }
        IsBusy = true;
        try
        {
            var result = await _apiClient.PostAsync($"api/v1/buses/{bus.Id}/deactivate", null);
            if (result is { IsSuccess: true })
            {
                await LoadAsync();
            }
            else
            {
                StatusMessage = result?.Message ?? "Unable to deactivate this bus.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task<bool> HandleWriteResultAsync<T>(ApiEnvelope<T>? result)
    {
        if (result is null)
        {
            StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
            return Task.FromResult(false);
        }
        if (!result.IsSuccess)
        {
            StatusMessage = result.Errors.Count > 0 ? string.Join("; ", result.Errors) : result.Message;
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }
}

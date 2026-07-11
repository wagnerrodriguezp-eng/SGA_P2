using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGA.Desktop.Wpf.Models;
using SGA.Desktop.Wpf.Services;

namespace SGA.Desktop.Wpf.ViewModels;

public partial class DriversViewModel : ObservableObject
{
    private readonly ISgaDesktopApiClient _apiClient;

    [ObservableProperty] private ObservableCollection<DriverProfileModel> drivers = new();
    [ObservableProperty] private DriverProfileModel? selectedDriver;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private string? statusMessage;

    [ObservableProperty] private string licenseNumber = string.Empty;
    [ObservableProperty] private string? phoneNumber;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string firstName = string.Empty;
    [ObservableProperty] private string lastName = string.Empty;

    public DriversViewModel(ISgaDesktopApiClient apiClient)
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
            var result = await _apiClient.GetAsync<List<DriverProfileModel>>("api/v1/drivers");
            if (result is null)
            {
                StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
                return;
            }
            Drivers = new ObservableCollection<DriverProfileModel>(result.Data ?? new List<DriverProfileModel>());
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void New()
    {
        SelectedDriver = null;
        IsEditing = true;
        LicenseNumber = string.Empty;
        PhoneNumber = null;
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    [RelayCommand]
    private void Edit(DriverProfileModel? driver)
    {
        if (driver is null)
        {
            return;
        }
        SelectedDriver = driver;
        IsEditing = true;
        LicenseNumber = driver.LicenseNumber;
        PhoneNumber = driver.PhoneNumber;
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
            if (SelectedDriver is null)
            {
                var result = await _apiClient.PostAsync<DriverProfileModel>("api/v1/drivers", new
                {
                    licenseNumber = LicenseNumber,
                    phoneNumber = PhoneNumber,
                    email = Email,
                    firstName = FirstName,
                    lastName = LastName
                });
                if (result is null)
                {
                    StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
                    return;
                }
                if (!result.IsSuccess)
                {
                    StatusMessage = result.Errors.Count > 0 ? string.Join("; ", result.Errors) : result.Message;
                    return;
                }
            }
            else
            {
                var result = await _apiClient.PutAsync<DriverProfileModel>($"api/v1/drivers/{SelectedDriver.Id}", new
                {
                    licenseNumber = LicenseNumber,
                    phoneNumber = PhoneNumber
                });
                if (result is null)
                {
                    StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
                    return;
                }
                if (!result.IsSuccess)
                {
                    StatusMessage = result.Errors.Count > 0 ? string.Join("; ", result.Errors) : result.Message;
                    return;
                }
            }

            IsEditing = false;
            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeactivateAsync(DriverProfileModel? driver)
    {
        if (driver is null)
        {
            return;
        }
        IsBusy = true;
        try
        {
            var result = await _apiClient.PostAsync($"api/v1/drivers/{driver.Id}/deactivate", null);
            if (result is { IsSuccess: true })
            {
                await LoadAsync();
            }
            else
            {
                StatusMessage = result?.Message ?? "Unable to deactivate this driver.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}

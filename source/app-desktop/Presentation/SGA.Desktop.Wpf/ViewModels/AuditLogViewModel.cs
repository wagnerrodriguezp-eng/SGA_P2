using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGA.Desktop.Wpf.Models;
using SGA.Desktop.Wpf.Services;

namespace SGA.Desktop.Wpf.ViewModels;

public partial class AuditLogViewModel : ObservableObject
{
    private readonly ISgaDesktopApiClient _apiClient;

    [ObservableProperty] private ObservableCollection<AuditLogModel> entries = new();
    [ObservableProperty] private DateTime? fromDate;
    [ObservableProperty] private DateTime? toDate;
    [ObservableProperty] private string? userIdInput;
    [ObservableProperty] private string? action;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? statusMessage;

    public AuditLogViewModel(ISgaDesktopApiClient apiClient)
    {
        _apiClient = apiClient;
        _ = SearchAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var query = new List<string>();
            if (FromDate.HasValue) query.Add($"from={FromDate.Value:O}");
            if (ToDate.HasValue) query.Add($"to={ToDate.Value:O}");
            if (Guid.TryParse(UserIdInput, out var userId)) query.Add($"userId={userId}");
            if (!string.IsNullOrWhiteSpace(Action)) query.Add($"action={Uri.EscapeDataString(Action)}");

            var uri = "api/v1/audit-logs" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
            var result = await _apiClient.GetAsync<List<AuditLogModel>>(uri);
            if (result is null)
            {
                StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
                return;
            }
            Entries = new ObservableCollection<AuditLogModel>(result.Data ?? new List<AuditLogModel>());
        }
        finally
        {
            IsBusy = false;
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGA.Desktop.Wpf.Models;
using SGA.Desktop.Wpf.Services;

namespace SGA.Desktop.Wpf.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly ISgaDesktopApiClient _apiClient;

    [ObservableProperty] private DateTime dateFrom = DateTime.Today.AddMonths(-1);
    [ObservableProperty] private DateTime dateTo = DateTime.Today;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? statusMessage;

    [ObservableProperty] private UsageReportModel? usageReport;
    [ObservableProperty] private OccupancyReportModel? occupancyReport;
    [ObservableProperty] private PunctualityReportModel? punctualityReport;
    [ObservableProperty] private RevenueReportModel? revenueReport;

    public ReportsViewModel(ISgaDesktopApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [RelayCommand]
    private async Task GenerateAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var query = $"dateFrom={DateOnly.FromDateTime(DateFrom):yyyy-MM-dd}&dateTo={DateOnly.FromDateTime(DateTo):yyyy-MM-dd}";

            var usage = await _apiClient.GetAsync<UsageReportModel>($"api/v1/reports/usage?{query}");
            var occupancy = await _apiClient.GetAsync<OccupancyReportModel>($"api/v1/reports/occupancy?{query}");
            var punctuality = await _apiClient.GetAsync<PunctualityReportModel>($"api/v1/reports/punctuality?{query}");
            var revenue = await _apiClient.GetAsync<RevenueReportModel>($"api/v1/reports/revenue?{query}");

            if (usage is null || occupancy is null || punctuality is null || revenue is null)
            {
                StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
                return;
            }

            UsageReport = usage.Data;
            OccupancyReport = occupancy.Data;
            PunctualityReport = punctuality.Data;
            RevenueReport = revenue.Data;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGA.Desktop.Wpf.Models;
using SGA.Desktop.Wpf.Services;

namespace SGA.Desktop.Wpf.ViewModels;

public partial class AuthorizationsViewModel : ObservableObject
{
    private readonly ISgaDesktopApiClient _apiClient;

    [ObservableProperty] private string userIdInput = string.Empty;
    [ObservableProperty] private ObservableCollection<AuthorizationModel> authorizations = new();
    [ObservableProperty] private AuthorizationModel? selectedAuthorization;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private string? statusMessage;

    [ObservableProperty] private AuthorizationTypeOption authorizationType = AuthorizationTypeOption.MonthlyTicket;
    [ObservableProperty] private DateTime startDate = DateTime.Today;
    [ObservableProperty] private DateTime? endDate;
    [ObservableProperty] private int? balance;
    [ObservableProperty] private AuthorizationStatusOption authorizationStatus = AuthorizationStatusOption.Active;

    public IReadOnlyList<AuthorizationTypeOption> AuthorizationTypeOptions { get; } = Enum.GetValues<AuthorizationTypeOption>();
    public IReadOnlyList<AuthorizationStatusOption> AuthorizationStatusOptions { get; } = Enum.GetValues<AuthorizationStatusOption>();

    public AuthorizationsViewModel(ISgaDesktopApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (!Guid.TryParse(UserIdInput, out var userId))
        {
            StatusMessage = "Enter a valid user id (GUID).";
            return;
        }

        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _apiClient.GetAsync<List<AuthorizationModel>>($"api/v1/authorizations?userId={userId}");
            if (result is null)
            {
                StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
                return;
            }
            Authorizations = new ObservableCollection<AuthorizationModel>(result.Data ?? new List<AuthorizationModel>());
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void New()
    {
        if (!Guid.TryParse(UserIdInput, out _))
        {
            StatusMessage = "Enter a valid user id (GUID) first.";
            return;
        }
        SelectedAuthorization = null;
        IsEditing = true;
        AuthorizationType = AuthorizationTypeOption.MonthlyTicket;
        StartDate = DateTime.Today;
        EndDate = DateTime.Today.AddMonths(1);
        Balance = null;
    }

    [RelayCommand]
    private void Edit(AuthorizationModel? authorization)
    {
        if (authorization is null)
        {
            return;
        }
        SelectedAuthorization = authorization;
        IsEditing = true;
        AuthorizationType = authorization.AuthorizationType;
        StartDate = authorization.StartDate.ToDateTime(TimeOnly.MinValue);
        EndDate = authorization.EndDate?.ToDateTime(TimeOnly.MinValue);
        Balance = authorization.Balance;
        AuthorizationStatus = authorization.AuthorizationStatus;
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
            if (SelectedAuthorization is null)
            {
                if (!Guid.TryParse(UserIdInput, out var userId))
                {
                    StatusMessage = "Enter a valid user id (GUID).";
                    return;
                }
                var result = await _apiClient.PostAsync<AuthorizationModel>("api/v1/authorizations", new
                {
                    userId,
                    authorizationType = (int)AuthorizationType,
                    startDate = DateOnly.FromDateTime(StartDate),
                    endDate = EndDate.HasValue ? DateOnly.FromDateTime(EndDate.Value) : (DateOnly?)null,
                    balance = Balance
                });
                if (result is null || !result.IsSuccess)
                {
                    StatusMessage = result?.Message ?? "The service is temporarily unavailable. Please try again shortly.";
                    return;
                }
            }
            else
            {
                var result = await _apiClient.PutAsync<AuthorizationModel>($"api/v1/authorizations/{SelectedAuthorization.Id}", new
                {
                    startDate = DateOnly.FromDateTime(StartDate),
                    endDate = EndDate.HasValue ? DateOnly.FromDateTime(EndDate.Value) : (DateOnly?)null,
                    balance = Balance,
                    authorizationStatus = (int)AuthorizationStatus
                });
                if (result is null || !result.IsSuccess)
                {
                    StatusMessage = result?.Message ?? "The service is temporarily unavailable. Please try again shortly.";
                    return;
                }
            }

            IsEditing = false;
            await SearchAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAuthorizationAsync(AuthorizationModel? authorization)
    {
        if (authorization is null)
        {
            return;
        }
        IsBusy = true;
        try
        {
            var result = await _apiClient.PostAsync($"api/v1/authorizations/{authorization.Id}/cancel", null);
            if (result is { IsSuccess: true })
            {
                await SearchAsync();
            }
            else
            {
                StatusMessage = result?.Message ?? "Unable to cancel this authorization.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}

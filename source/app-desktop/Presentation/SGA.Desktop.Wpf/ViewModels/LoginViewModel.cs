using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGA.Desktop.Wpf.Services;

namespace SGA.Desktop.Wpf.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ISgaDesktopApiClient _apiClient;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool isBusy;

    [ObservableProperty]
    private string? statusMessage;

    public event EventHandler? LoginSucceeded;

    public LoginViewModel(ISgaDesktopApiClient apiClient, ICurrentUserService currentUserService)
    {
        _apiClient = apiClient;
        _currentUserService = currentUserService;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _apiClient.LoginAsync(Email, Password);
            if (result is null)
            {
                StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
                return;
            }
            if (!result.IsSuccess || result.Data is null)
            {
                StatusMessage = "Invalid email or password.";
                return;
            }

            _currentUserService.SetSession(result.Data.AccessToken, result.Data.RefreshToken, result.Data.AccessTokenExpiresAtUtc);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
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

    private bool CanLogin() => !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
}

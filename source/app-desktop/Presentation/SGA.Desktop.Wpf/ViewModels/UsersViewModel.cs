using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGA.Desktop.Wpf.Models;
using SGA.Desktop.Wpf.Services;

namespace SGA.Desktop.Wpf.ViewModels;

// Administrative provisioning for Student/Employee/Driver accounts — the credential still only
// ever works on app-web-mvc, per the application-affinity rule.
public partial class UsersViewModel : ObservableObject
{
    private readonly ISgaDesktopApiClient _apiClient;

    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string firstName = string.Empty;
    [ObservableProperty] private string lastName = string.Empty;
    [ObservableProperty] private RegistrationRoleOption role = RegistrationRoleOption.Student;
    [ObservableProperty] private string? studentCode;
    [ObservableProperty] private string? employeeCode;
    [ObservableProperty] private string? licenseNumber;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? statusMessage;

    public IReadOnlyList<RegistrationRoleOption> RoleOptions { get; } = Enum.GetValues<RegistrationRoleOption>();

    public UsersViewModel(ISgaDesktopApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [RelayCommand]
    private async Task ProvisionAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var result = await _apiClient.PostAsync("api/v1/admin/users", new
            {
                email = Email,
                firstName = FirstName,
                lastName = LastName,
                role = (int)Role,
                studentCode = StudentCode,
                employeeCode = EmployeeCode,
                licenseNumber = LicenseNumber
            });

            if (result is null)
            {
                StatusMessage = "The service is temporarily unavailable. Please try again shortly.";
                return;
            }

            if (result.IsSuccess)
            {
                StatusMessage = result.Message ?? "Account provisioned.";
                Email = string.Empty;
                FirstName = string.Empty;
                LastName = string.Empty;
                StudentCode = null;
                EmployeeCode = null;
                LicenseNumber = null;
            }
            else
            {
                StatusMessage = result.Errors.Count > 0 ? string.Join("; ", result.Errors) : "Unable to provision account.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}

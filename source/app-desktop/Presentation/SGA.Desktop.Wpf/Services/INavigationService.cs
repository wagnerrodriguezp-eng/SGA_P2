namespace SGA.Desktop.Wpf.Services;

public interface INavigationService
{
    object? CurrentView { get; }
    event Action? CurrentViewChanged;
    void NavigateTo<TViewModel>() where TViewModel : class;
}

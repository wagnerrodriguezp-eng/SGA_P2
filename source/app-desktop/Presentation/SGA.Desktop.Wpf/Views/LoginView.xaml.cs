using System.Windows;
using System.Windows.Controls;
using SGA.Desktop.Wpf.ViewModels;

namespace SGA.Desktop.Wpf.Views;

public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
        {
            viewModel.Password = ((PasswordBox)sender).Password;
        }
    }
}

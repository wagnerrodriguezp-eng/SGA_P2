using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SGA.Desktop.Wpf.Converters;

// Visible when the bound value is a non-null, non-empty string (or any other non-null object) —
// used to show/hide a status message TextBlock without an extra bool property per ViewModel.
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is string s
            ? (string.IsNullOrWhiteSpace(s) ? Visibility.Collapsed : Visibility.Visible)
            : (value is null ? Visibility.Collapsed : Visibility.Visible);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace UniversalExcelTool.UI.Converters
{
    /// <summary>
    /// Converts boolean to notification background color (success = green, failure = red)
    /// </summary>
    public class BoolToNotificationColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool success)
            {
                return success 
                    ? new SolidColorBrush(Color.Parse("#DCFCE7")) // Light green
                    : new SolidColorBrush(Color.Parse("#FEE2E2")); // Light red
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean to notification border color (success = green, failure = red)
    /// </summary>
    public class BoolToNotificationBorderConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool success)
            {
                return success 
                    ? new SolidColorBrush(Color.Parse("#10B981")) // Green
                    : new SolidColorBrush(Color.Parse("#EF4444")); // Red
            }
            return new SolidColorBrush(Color.Parse("#E5E7EB"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean to notification text color (success = dark green, failure = dark red)
    /// </summary>
    public class BoolToNotificationTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool success)
            {
                return success 
                    ? new SolidColorBrush(Color.Parse("#166534")) // Dark green
                    : new SolidColorBrush(Color.Parse("#991B1B")); // Dark red
            }
            return new SolidColorBrush(Color.Parse("#1A1A1A"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean to icon (success = checkmark, failure = X)
    /// </summary>
    public class BoolToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool success)
            {
                return success ? "✅" : "❌";
            }
            return "ℹ️";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

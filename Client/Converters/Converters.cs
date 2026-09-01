using System.Globalization;

namespace SocialMediaSimulator.Client.Converters;

/// <summary>
/// Converts a boolean to text based on a pipe-separated parameter (TrueText|FalseText)
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isTrue = value is bool b && b;
        string? param = parameter as string;
        if (string.IsNullOrEmpty(param)) return isTrue ? "True" : "False";
        
        var parts = param.Split('|');
        return isTrue ? (parts.Length > 0 ? parts[0] : "True") : (parts.Length > 1 ? parts[1] : "False");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Inverts a boolean value
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : true;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}

/// <summary>
/// Converts a string to boolean (true if not null/empty)
/// </summary>
public class StringToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrEmpty(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts zero to true (for visibility of empty state)
/// </summary>
public class ZeroToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i) return i == 0;
        return value == null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

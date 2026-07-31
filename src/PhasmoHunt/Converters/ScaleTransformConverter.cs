using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PhasmoHunt.Converters;

public sealed class ScaleTransformConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var scale = value is double d ? d : 1.0;
        return new ScaleTransform(scale, scale);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

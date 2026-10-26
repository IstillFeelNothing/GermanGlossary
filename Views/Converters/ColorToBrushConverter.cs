using System.Globalization;

namespace GermanyGlossary.Views.Converters
{
    // Lets a Color property (e.g. RadioButton.BorderColor) drive a Brush property (e.g. Border.Stroke).
    public class ColorToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is Color color ? new SolidColorBrush(color) : null;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}

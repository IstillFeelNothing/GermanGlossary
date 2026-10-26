using System.Globalization;

namespace GermanyGlossary.Views.Converters
{
    // "der" / "die" / "das" -> the matching Article* color from Colors.xaml.
    // Anything else (null for non-nouns) -> the color whose key is passed as ConverterParameter,
    // or TextPrimary when there is none.
    public class ArticleColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var key = (value as string) switch
            {
                "der" => "ArticleDer",
                "die" => "ArticleDie",
                "das" => "ArticleDas",
                _ => parameter as string ?? "TextPrimary"
            };

            if (Application.Current?.Resources.TryGetValue(key, out var resource) != true || resource is not Color color)
                return null;

            // Border.Stroke and similar properties are Brush, not Color.
            return typeof(Brush).IsAssignableFrom(targetType) ? new SolidColorBrush(color) : color;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}

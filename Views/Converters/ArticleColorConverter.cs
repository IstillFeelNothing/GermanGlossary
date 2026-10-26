using System.Globalization;

namespace GermanyGlossary.Views.Converters
{
    // "der" / "die" / "das" -> the matching Article* color from Colors.xaml.
    public class ArticleColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var key = (value as string) switch
            {
                "der" => "ArticleDer",
                "die" => "ArticleDie",
                "das" => "ArticleDas",
                _ => "TextPrimary"
            };

            return Application.Current?.Resources.TryGetValue(key, out var color) == true ? color : null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}

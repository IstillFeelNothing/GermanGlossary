using GermanyGlossary.Models;

namespace GermanyGlossary.ViewModels
{
    public record WordListItem(int Id, string Text, string FullText, string Translation, string KindLabel)
    {
        public static WordListItem From(Word word) => new(
            word.Id,
            word.Text,
            WordFormatter.FullText(word),
            word.Translation,
            WordFormatter.KindLabel(word));

        public bool Matches(string query) =>
            FullText.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
            Translation.Contains(query, StringComparison.CurrentCultureIgnoreCase);
    }
}

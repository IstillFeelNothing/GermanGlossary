using GermanyGlossary.Models;

namespace GermanyGlossary.ViewModels
{
    public record WordListItem(int Id, string? Article, string Text, string FullText, string Translation, string KindLabel)
    {
        public bool HasArticle => Article is not null;

        // "der " (with a trailing space) for nouns, empty otherwise; shown in the article color.
        public string ArticlePrefix => Article is null ? string.Empty : $"{Article} ";

        public static WordListItem From(Word word) => new(
            word.Id,
            WordFormatter.ArticleOf(word),
            word.Text,
            WordFormatter.FullText(word),
            word.Translation,
            WordFormatter.KindLabel(word));

        public bool Matches(string query) =>
            FullText.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
            Translation.Contains(query, StringComparison.CurrentCultureIgnoreCase);
    }
}

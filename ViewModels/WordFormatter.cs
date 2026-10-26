using GermanyGlossary.Models;

namespace GermanyGlossary.ViewModels
{
    // Turns domain words into the strings shown on screens.
    public static class WordFormatter
    {
        public static string ArticleText(Article article) => article switch
        {
            Article.Der => "der",
            Article.Die => "die",
            Article.Das => "das",
            _ => string.Empty
        };

        // "der Apfel" for nouns, the plain text for everything else.
        public static string FullText(Word word) =>
            word is Noun noun ? $"{ArticleText(noun.Article)} {noun.Text}" : word.Text;

        public static string KindLabel(Word word) => word switch
        {
            Noun => "Іменник",
            Verb => "Дієслово",
            Adjective => "Прикметник",
            Phrase => "Фраза",
            _ => string.Empty
        };

        public static string? GrammarDetails(Word word)
        {
            var parts = new List<string>();

            switch (word)
            {
                case Noun noun:
                    if (!string.IsNullOrWhiteSpace(noun.Plural))
                        parts.Add($"мн.: die {noun.Plural}");
                    break;
                case Verb verb:
                    if (!string.IsNullOrWhiteSpace(verb.Praeteritum))
                        parts.Add(verb.Praeteritum);
                    if (!string.IsNullOrWhiteSpace(verb.PartizipII))
                        parts.Add($"{(verb.Auxiliary == AuxiliaryVerb.Sein ? "ist" : "hat")} {verb.PartizipII}");
                    if (verb.IsSeparable)
                        parts.Add("відокремлюване");
                    break;
                case Adjective adjective:
                    if (!string.IsNullOrWhiteSpace(adjective.Comparative))
                        parts.Add(adjective.Comparative);
                    if (!string.IsNullOrWhiteSpace(adjective.Superlative))
                        parts.Add(adjective.Superlative);
                    break;
            }

            return parts.Count == 0 ? null : string.Join(" · ", parts);
        }
    }
}

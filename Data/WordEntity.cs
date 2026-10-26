using SQLite;

namespace GermanyGlossary.Data
{
    // Persistence shape of Word: all subtypes share one table, subtype-specific columns are nullable.
    [Table("Words")]
    public class WordEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public WordKind Kind { get; set; }

        [NotNull]
        public string Text { get; set; } = string.Empty;

        [NotNull]
        public string Translation { get; set; } = string.Empty;

        public string? Example { get; set; }

        [Indexed]
        public int DeckId { get; set; }

        // Stored as UTC ticks so DateTimeKind survives the round trip.
        public long CreatedAtUtcTicks { get; set; }

        // Noun
        public int? Article { get; set; }
        public string? Plural { get; set; }

        // Verb
        public string? Praeteritum { get; set; }
        public string? PartizipII { get; set; }
        public int? Auxiliary { get; set; }
        public bool? IsSeparable { get; set; }

        // Adjective
        public string? Comparative { get; set; }
        public string? Superlative { get; set; }
    }
}

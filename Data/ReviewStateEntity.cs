using SQLite;

namespace GermanyGlossary.Data
{
    [Table("ReviewStates")]
    public class ReviewStateEntity
    {
        // One row per word, so the word id doubles as the primary key.
        [PrimaryKey]
        public int WordId { get; set; }

        public int Interval { get; set; }

        public double EaseFactor { get; set; }

        public int Repetitions { get; set; }

        // UTC ticks, same convention as WordEntity.CreatedAtUtcTicks.
        [Indexed]
        public long NextReviewAtUtcTicks { get; set; }

        public long? LastReviewedAtUtcTicks { get; set; }
    }
}

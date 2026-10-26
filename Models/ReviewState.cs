namespace GermanyGlossary.Models
{
    // Spaced-repetition progress of one word. Kept apart from Word so the algorithm can change
    // without touching vocabulary data. Immutable: the algorithm returns a new instance.
    public record ReviewState
    {
        public const double DefaultEaseFactor = 2.5;

        public int WordId { get; init; }

        // Days until the next review.
        public int Interval { get; init; }

        public double EaseFactor { get; init; } = DefaultEaseFactor;

        // Successful reviews in a row; reset to 0 on "Again".
        public int Repetitions { get; init; }

        public DateTime NextReviewAt { get; init; }

        public DateTime? LastReviewedAt { get; init; }

        public bool IsNew => LastReviewedAt is null;

        public static ReviewState CreateNew(int wordId, DateTime now) => new()
        {
            WordId = wordId,
            NextReviewAt = now
        };
    }
}

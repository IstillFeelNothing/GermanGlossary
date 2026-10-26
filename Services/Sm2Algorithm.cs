using GermanyGlossary.Models;

namespace GermanyGlossary.Services
{
    // SuperMemo SM-2 (https://super-memory.com/english/ol/sm2.htm) with the four app ratings
    // mapped onto SM-2 quality grades: Again = 1, Hard = 3, Good = 4, Easy = 5.
    public class Sm2Algorithm : ISpacedRepetitionAlgorithm
    {
        public const double MinEaseFactor = 1.3;

        private const int FirstInterval = 1;
        private const int SecondInterval = 6;

        private readonly IClock _clock;

        public Sm2Algorithm(IClock clock)
        {
            _clock = clock;
        }

        public ReviewState Apply(ReviewState state, ReviewRating rating)
        {
            ArgumentNullException.ThrowIfNull(state);

            var now = _clock.UtcNow;
            var quality = ToQuality(rating);

            int repetitions;
            int interval;
            var easeFactor = state.EaseFactor;

            if (quality < 3)
            {
                // Forgotten: start the repetition sequence over, ease factor stays as it was.
                repetitions = 0;
                interval = FirstInterval;
            }
            else
            {
                interval = state.Repetitions switch
                {
                    0 => FirstInterval,
                    1 => SecondInterval,
                    _ => (int)Math.Round(state.Interval * state.EaseFactor, MidpointRounding.AwayFromZero)
                };
                repetitions = state.Repetitions + 1;
                easeFactor = NextEaseFactor(state.EaseFactor, quality);
            }

            return state with
            {
                Interval = interval,
                EaseFactor = easeFactor,
                Repetitions = repetitions,
                LastReviewedAt = now,
                NextReviewAt = now.AddDays(interval)
            };
        }

        private static double NextEaseFactor(double easeFactor, int quality)
        {
            var next = easeFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
            return Math.Max(MinEaseFactor, next);
        }

        private static int ToQuality(ReviewRating rating) => rating switch
        {
            ReviewRating.Again => 1,
            ReviewRating.Hard => 3,
            ReviewRating.Good => 4,
            ReviewRating.Easy => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(rating), rating, null)
        };
    }
}

using GermanyGlossary.Models;

namespace GermanyGlossary.Services
{
    // A word in a review session together with its current spaced-repetition state.
    public class ReviewCard
    {
        public ReviewCard(Word word, ReviewState state)
        {
            Word = word;
            State = state;
        }

        public Word Word { get; }
        public ReviewState State { get; internal set; }
    }

    public interface IReviewSessionService
    {
        // Cards due today (most overdue first), followed by a limited number of new words.
        Task<IReadOnlyList<ReviewCard>> StartSessionAsync();

        // Runs the rating through the algorithm, saves the result and updates card.State.
        Task RateAsync(ReviewCard card, ReviewRating rating);

        // What each rating would do to the card, without saving anything (for the button hints).
        IReadOnlyDictionary<ReviewRating, ReviewState> Preview(ReviewCard card);

        // The earliest upcoming review, or null if nothing has been studied yet.
        Task<DateTime?> GetNextReviewAtAsync();
    }
}

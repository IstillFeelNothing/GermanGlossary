using GermanyGlossary.Models;
using GermanyGlossary.Repositories;

namespace GermanyGlossary.Services
{
    public class ReviewSessionService : IReviewSessionService
    {
        // How many never-studied words are introduced per session, so a big import doesn't flood one day.
        public const int NewCardsPerSession = 20;

        private readonly IWordRepository _wordRepository;
        private readonly IReviewStateRepository _reviewStateRepository;
        private readonly ISpacedRepetitionAlgorithm _algorithm;
        private readonly IClock _clock;

        public ReviewSessionService(
            IWordRepository wordRepository,
            IReviewStateRepository reviewStateRepository,
            ISpacedRepetitionAlgorithm algorithm,
            IClock clock)
        {
            _wordRepository = wordRepository;
            _reviewStateRepository = reviewStateRepository;
            _algorithm = algorithm;
            _clock = clock;
        }

        public async Task<IReadOnlyList<ReviewCard>> StartSessionAsync()
        {
            var now = _clock.UtcNow;
            var words = (await _wordRepository.GetAllAsync()).ToDictionary(w => w.Id);
            var states = await _reviewStateRepository.GetAllAsync();
            var studiedIds = states.Select(s => s.WordId).ToHashSet();

            // "Due today" means anything scheduled before the end of the local day,
            // so a word reviewed yesterday evening shows up this morning.
            var endOfToday = EndOfLocalDay(now);
            var dueCards = (await _reviewStateRepository.GetDueAsync(endOfToday))
                .Where(s => words.ContainsKey(s.WordId))
                .Select(s => new ReviewCard(words[s.WordId], s));

            var newWords = words.Values
                .Where(w => !studiedIds.Contains(w.Id))
                .ToArray();
            Random.Shared.Shuffle(newWords);
            var newCards = newWords
                .Take(NewCardsPerSession)
                .Select(w => new ReviewCard(w, ReviewState.CreateNew(w.Id, now)));

            return dueCards.Concat(newCards).ToList();
        }

        public async Task RateAsync(ReviewCard card, ReviewRating rating)
        {
            ArgumentNullException.ThrowIfNull(card);

            var next = _algorithm.Apply(card.State, rating);
            await _reviewStateRepository.SaveAsync(next);
            card.State = next;
        }

        public IReadOnlyDictionary<ReviewRating, ReviewState> Preview(ReviewCard card)
        {
            ArgumentNullException.ThrowIfNull(card);

            return Enum.GetValues<ReviewRating>()
                .ToDictionary(rating => rating, rating => _algorithm.Apply(card.State, rating));
        }

        public async Task<DateTime?> GetNextReviewAtAsync()
        {
            var states = await _reviewStateRepository.GetAllAsync();
            return states.Count == 0 ? null : states.Min(s => s.NextReviewAt);
        }

        private static DateTime EndOfLocalDay(DateTime utcNow) =>
            utcNow.ToLocalTime().Date.AddDays(1).ToUniversalTime();
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GermanyGlossary.Models;
using GermanyGlossary.Services;

namespace GermanyGlossary.ViewModels
{
    // Review session driven by SM-2: every rating is saved through IReviewSessionService.
    // "Again" also sends the card to the back of the queue so it comes up once more today.
    public partial class FlashcardsViewModel : ObservableObject
    {
        private readonly IReviewSessionService _sessionService;
        private readonly Queue<ReviewCard> _queue = new();
        private readonly HashSet<int> _missedWordIds = [];
        private int _totalCount;
        private bool _isBusy;

        public FlashcardsViewModel(IReviewSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentWord), nameof(HasCard), nameof(FrontText), nameof(Article),
            nameof(ArticlePrefix), nameof(KindLabel), nameof(Translation), nameof(Details), nameof(Example),
            nameof(HasExample), nameof(IsNewCard), nameof(RevealedArticle))]
        public partial ReviewCard? CurrentCard { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFront), nameof(RevealedArticle))]
        public partial bool IsFlipped { get; set; }

        // No words in the dictionary at all.
        [ObservableProperty]
        public partial bool IsEmpty { get; set; }

        // Words exist but nothing is left for today (either right away or after finishing the session).
        [ObservableProperty]
        public partial bool IsDone { get; set; }

        [ObservableProperty]
        public partial string DoneTitle { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool HasSessionStats { get; set; }

        [ObservableProperty]
        public partial string NextReviewText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ProgressText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial double Progress { get; set; }

        [ObservableProperty]
        public partial int KnownCount { get; set; }

        [ObservableProperty]
        public partial int MissedCount { get; set; }

        [ObservableProperty]
        public partial string HardHint { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string GoodHint { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string EasyHint { get; set; } = string.Empty;

        public Word? CurrentWord => CurrentCard?.Word;
        public bool HasCard => CurrentCard is not null;
        public bool IsFront => !IsFlipped;
        public bool IsNewCard => CurrentCard?.State.IsNew == true;

        public string FrontText => CurrentWord?.Text ?? string.Empty;
        public string? Article => CurrentWord is null ? null : WordFormatter.ArticleOf(CurrentWord);
        public string ArticlePrefix => Article is null ? string.Empty : $"{Article} ";
        public string KindLabel => CurrentWord is null ? string.Empty : WordFormatter.KindLabel(CurrentWord);
        public string Translation => CurrentWord?.Translation ?? string.Empty;
        public string? Details => CurrentWord is null ? null : WordFormatter.GrammarDetails(CurrentWord);
        public string? Example => CurrentWord?.Example;
        public bool HasExample => !string.IsNullOrWhiteSpace(Example);

        // The article only once the answer is shown, so the front doesn't give the gender away.
        public string? RevealedArticle => IsFlipped ? Article : null;

        // Starts a session unless one is already running or its summary is on screen.
        public async Task EnsureStartedAsync()
        {
            if (HasCard || IsDone)
                return;

            await RestartAsync();
        }

        [RelayCommand]
        private async Task RestartAsync()
        {
            var cards = await _sessionService.StartSessionAsync();

            _queue.Clear();
            _missedWordIds.Clear();
            foreach (var card in cards)
                _queue.Enqueue(card);

            _totalCount = cards.Count;
            HasSessionStats = false;
            IsDone = false;
            IsEmpty = false;

            if (_totalCount == 0)
            {
                var nextReviewAt = await _sessionService.GetNextReviewAtAsync();
                if (nextReviewAt is null)
                {
                    // Nothing studied and nothing new to study: the dictionary is empty.
                    IsEmpty = true;
                }
                else
                {
                    ShowDone("На сьогодні все", nextReviewAt);
                }
            }

            ShowNext();
        }

        [RelayCommand]
        private void Flip()
        {
            if (HasCard)
                IsFlipped = !IsFlipped;
        }

        [RelayCommand]
        private Task AgainAsync() => RateAsync(ReviewRating.Again);

        [RelayCommand]
        private Task HardAsync() => RateAsync(ReviewRating.Hard);

        [RelayCommand]
        private Task GoodAsync() => RateAsync(ReviewRating.Good);

        [RelayCommand]
        private Task EasyAsync() => RateAsync(ReviewRating.Easy);

        private async Task RateAsync(ReviewRating rating)
        {
            if (CurrentCard is null || _isBusy)
                return;

            _isBusy = true;
            try
            {
                var card = _queue.Dequeue();
                await _sessionService.RateAsync(card, rating);

                if (rating == ReviewRating.Again)
                {
                    _missedWordIds.Add(card.Word.Id);
                    _queue.Enqueue(card);
                }

                ShowNext();

                if (CurrentCard is null)
                {
                    HasSessionStats = true;
                    MissedCount = _missedWordIds.Count;
                    KnownCount = _totalCount - MissedCount;
                    ShowDone("Готово!", await _sessionService.GetNextReviewAtAsync());
                }
            }
            finally
            {
                _isBusy = false;
            }
        }

        private void ShowNext()
        {
            IsFlipped = false;
            CurrentCard = _queue.Count > 0 ? _queue.Peek() : null;

            // A card answered "Again" stays in the queue, so count distinct words that are finished.
            var done = _totalCount - _queue.DistinctBy(c => c.Word.Id).Count();
            ProgressText = $"{done} / {_totalCount}";
            Progress = _totalCount == 0 ? 0 : (double)done / _totalCount;

            if (CurrentCard is not null)
                UpdateHints(CurrentCard);
        }

        private void UpdateHints(ReviewCard card)
        {
            var preview = _sessionService.Preview(card);
            HardHint = FormatInterval(preview[ReviewRating.Hard].Interval);
            GoodHint = FormatInterval(preview[ReviewRating.Good].Interval);
            EasyHint = FormatInterval(preview[ReviewRating.Easy].Interval);
        }

        private void ShowDone(string title, DateTime? nextReviewAt)
        {
            IsDone = true;
            DoneTitle = title;
            NextReviewText = nextReviewAt is null ? string.Empty : $"Наступне повторення: {FormatDay(nextReviewAt.Value)}";
        }

        private static string FormatInterval(int days) => days switch
        {
            < 30 => $"{days} д",
            < 365 => $"{Math.Round(days / 30.0, 1)} міс",
            _ => $"{Math.Round(days / 365.0, 1)} р"
        };

        private static string FormatDay(DateTime utc)
        {
            var days = (utc.ToLocalTime().Date - DateTime.Today).Days;
            return days switch
            {
                <= 0 => "сьогодні",
                1 => "завтра",
                _ => $"через {days} дн."
            };
        }
    }
}

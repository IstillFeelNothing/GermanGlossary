using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GermanyGlossary.Models;
using GermanyGlossary.Repositories;

namespace GermanyGlossary.ViewModels
{
    // A simple in-memory session: "don't know" sends the card to the back of the queue,
    // "know" removes it. Nothing is persisted yet (ReviewState / SM-2 come later).
    public partial class FlashcardsViewModel : ObservableObject
    {
        private readonly IWordRepository _wordRepository;
        private readonly Queue<Word> _queue = new();
        private readonly HashSet<int> _missedWordIds = [];
        private int _totalCount;

        public FlashcardsViewModel(IWordRepository wordRepository)
        {
            _wordRepository = wordRepository;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasCard), nameof(FrontText), nameof(Article), nameof(ArticlePrefix),
            nameof(KindLabel), nameof(Translation), nameof(Details), nameof(Example), nameof(HasExample))]
        public partial Word? CurrentWord { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFront))]
        public partial bool IsFlipped { get; set; }

        [ObservableProperty]
        public partial bool IsEmpty { get; set; }

        [ObservableProperty]
        public partial bool IsFinished { get; set; }

        [ObservableProperty]
        public partial string ProgressText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial double Progress { get; set; }

        [ObservableProperty]
        public partial string SummaryText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial int KnownCount { get; set; }

        [ObservableProperty]
        public partial int MissedCount { get; set; }

        public bool HasCard => CurrentWord is not null;
        public bool IsFront => !IsFlipped;

        public string FrontText => CurrentWord?.Text ?? string.Empty;
        public string? Article => CurrentWord is null ? null : WordFormatter.ArticleOf(CurrentWord);
        public string ArticlePrefix => Article is null ? string.Empty : $"{Article} ";
        public string KindLabel => CurrentWord is null ? string.Empty : WordFormatter.KindLabel(CurrentWord);
        public string Translation => CurrentWord?.Translation ?? string.Empty;
        public string? Details => CurrentWord is null ? null : WordFormatter.GrammarDetails(CurrentWord);
        public string? Example => CurrentWord?.Example;
        public bool HasExample => !string.IsNullOrWhiteSpace(Example);

        // Starts a session unless one is already in progress or showing its summary.
        public async Task EnsureStartedAsync()
        {
            if (HasCard || IsFinished)
                return;

            await RestartAsync();
        }

        [RelayCommand]
        private async Task RestartAsync()
        {
            var words = (await _wordRepository.GetAllAsync()).ToArray();
            Random.Shared.Shuffle(words);

            _queue.Clear();
            _missedWordIds.Clear();
            foreach (var word in words)
                _queue.Enqueue(word);

            _totalCount = words.Length;
            IsEmpty = _totalCount == 0;
            IsFinished = false;
            ShowNext();
        }

        [RelayCommand]
        private void Flip()
        {
            if (HasCard)
                IsFlipped = !IsFlipped;
        }

        [RelayCommand]
        private void Know()
        {
            if (CurrentWord is null)
                return;

            _queue.Dequeue();
            ShowNext();
        }

        [RelayCommand]
        private void DontKnow()
        {
            if (CurrentWord is null)
                return;

            _missedWordIds.Add(CurrentWord.Id);
            _queue.Enqueue(_queue.Dequeue());
            ShowNext();
        }

        private void ShowNext()
        {
            IsFlipped = false;
            CurrentWord = _queue.Count > 0 ? _queue.Peek() : null;
            var done = _totalCount - _queue.Count;
            ProgressText = $"{done} / {_totalCount}";
            Progress = _totalCount == 0 ? 0 : (double)done / _totalCount;

            if (CurrentWord is null && _totalCount > 0)
            {
                IsFinished = true;
                MissedCount = _missedWordIds.Count;
                KnownCount = _totalCount - MissedCount;
                SummaryText = $"Ви пройшли всі {_totalCount} карток";
            }
        }
    }
}

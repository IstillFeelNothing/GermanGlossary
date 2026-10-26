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
        [NotifyPropertyChangedFor(nameof(HasCard), nameof(FrontText), nameof(FullText),
            nameof(Translation), nameof(Details), nameof(Example))]
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
        public partial string SummaryText { get; set; } = string.Empty;

        public bool HasCard => CurrentWord is not null;
        public bool IsFront => !IsFlipped;

        public string FrontText => CurrentWord?.Text ?? string.Empty;
        public string FullText => CurrentWord is null ? string.Empty : WordFormatter.FullText(CurrentWord);
        public string Translation => CurrentWord?.Translation ?? string.Empty;
        public string? Details => CurrentWord is null ? null : WordFormatter.GrammarDetails(CurrentWord);
        public string? Example => CurrentWord?.Example;

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
            ProgressText = $"Залишилось: {_queue.Count} з {_totalCount}";

            if (CurrentWord is null && _totalCount > 0)
            {
                IsFinished = true;
                var knownFirstTime = _totalCount - _missedWordIds.Count;
                SummaryText = $"Знали з першого разу: {knownFirstTime} з {_totalCount}";
            }
        }
    }
}

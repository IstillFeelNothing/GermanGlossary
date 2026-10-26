using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GermanyGlossary.Repositories;
using GermanyGlossary.Views;

namespace GermanyGlossary.ViewModels
{
    public partial class WordsViewModel : ObservableObject
    {
        private readonly IWordRepository _wordRepository;
        private List<WordListItem> _allWords = [];

        public WordsViewModel(IWordRepository wordRepository)
        {
            _wordRepository = wordRepository;
        }

        public ObservableCollection<WordListItem> Words { get; } = [];

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial WordListItem? SelectedWord { get; set; }

        [ObservableProperty]
        public partial string EmptyTitle { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string EmptyMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string CountText { get; set; } = string.Empty;

        [RelayCommand]
        public async Task LoadAsync()
        {
            var words = await _wordRepository.GetAllAsync();
            _allWords = words.Select(WordListItem.From).ToList();
            ApplyFilter();
        }

        [RelayCommand]
        private Task AddAsync() => Shell.Current.GoToAsync(nameof(WordEditPage));

        partial void OnSearchTextChanged(string value) => ApplyFilter();

        partial void OnSelectedWordChanged(WordListItem? value)
        {
            if (value is null)
                return;

            SelectedWord = null;
            Shell.Current.GoToAsync($"{nameof(WordEditPage)}?id={value.Id}");
        }

        private void ApplyFilter()
        {
            var query = SearchText.Trim();
            var filtered = query.Length == 0
                ? _allWords
                : _allWords.Where(w => w.Matches(query));

            Words.Clear();
            foreach (var word in filtered)
                Words.Add(word);

            var isDictionaryEmpty = _allWords.Count == 0;
            EmptyTitle = isDictionaryEmpty ? "Словник порожній" : "Нічого не знайдено";
            EmptyMessage = isDictionaryEmpty
                ? "Натисніть «Додати», щоб створити перше слово."
                : "Спробуйте інший запит.";
            CountText = FormatWordCount(_allWords.Count);
        }

        private static string FormatWordCount(int count)
        {
            // Ukrainian plural forms: 1 слово, 2–4 слова, 5+ слів (11–14 are always "слів").
            var lastTwo = count % 100;
            var last = count % 10;
            var noun = lastTwo is >= 11 and <= 14 ? "слів"
                : last == 1 ? "слово"
                : last is >= 2 and <= 4 ? "слова"
                : "слів";

            return $"{count} {noun}";
        }
    }
}

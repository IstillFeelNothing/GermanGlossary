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
        public partial string EmptyMessage { get; set; } = string.Empty;

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

            EmptyMessage = _allWords.Count == 0
                ? "Словник порожній. Натисніть «Додати», щоб створити перше слово."
                : "Нічого не знайдено.";
        }
    }
}

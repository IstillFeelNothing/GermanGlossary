using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GermanyGlossary.Models;
using GermanyGlossary.Repositories;

namespace GermanyGlossary.ViewModels
{
    public partial class WordEditViewModel : ObservableObject, IQueryAttributable
    {
        // Values of the chip groups on the page (RadioButton.Value).
        public const string NounKind = "noun";
        public const string VerbKind = "verb";
        public const string AdjectiveKind = "adjective";
        public const string PhraseKind = "phrase";

        private readonly IWordRepository _wordRepository;
        private int? _wordId;
        private DateTime _createdAt;
        private bool _isLoaded;

        public WordEditViewModel(IWordRepository wordRepository)
        {
            _wordRepository = wordRepository;
        }

        public bool IsEditMode => _wordId is not null;
        public string PageTitle => IsEditMode ? "Редагування" : "Нове слово";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNoun), nameof(IsVerb), nameof(IsAdjective))]
        public partial string SelectedKind { get; set; } = NounKind;

        public bool IsNoun => SelectedKind == NounKind;
        public bool IsVerb => SelectedKind == VerbKind;
        public bool IsAdjective => SelectedKind == AdjectiveKind;

        [ObservableProperty]
        public partial string Text { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Translation { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Example { get; set; } = string.Empty;

        // "der" / "die" / "das"
        [ObservableProperty]
        public partial string Article { get; set; } = "der";

        [ObservableProperty]
        public partial string Plural { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Praeteritum { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string PartizipII { get; set; } = string.Empty;

        // "haben" / "sein"
        [ObservableProperty]
        public partial string Auxiliary { get; set; } = "haben";

        [ObservableProperty]
        public partial bool IsSeparable { get; set; }

        [ObservableProperty]
        public partial string Comparative { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Superlative { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        public partial string? ErrorMessage { get; set; }

        public bool HasError => ErrorMessage is not null;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var value) && int.TryParse(value?.ToString(), out var id))
                _wordId = id;

            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(PageTitle));
        }

        public async Task LoadAsync()
        {
            if (_isLoaded)
                return;
            _isLoaded = true;

            if (_wordId is null)
                return;

            var word = await _wordRepository.GetByIdAsync(_wordId.Value);
            if (word is null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            Fill(word);
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Text) || string.IsNullOrWhiteSpace(Translation))
            {
                ErrorMessage = "Заповніть слово та переклад.";
                return;
            }

            ErrorMessage = null;
            var word = BuildWord();

            if (_wordId is null)
            {
                word.CreatedAt = DateTime.UtcNow;
                await _wordRepository.AddAsync(word);
            }
            else
            {
                word.Id = _wordId.Value;
                word.CreatedAt = _createdAt;
                await _wordRepository.UpdateAsync(word);
            }

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (_wordId is null)
                return;

            var confirmed = await Shell.Current.DisplayAlertAsync(
                "Видалити слово?", $"«{Text.Trim()}» буде видалено назавжди.", "Видалити", "Скасувати");
            if (!confirmed)
                return;

            await _wordRepository.DeleteAsync(_wordId.Value);
            await Shell.Current.GoToAsync("..");
        }

        private void Fill(Word word)
        {
            _createdAt = word.CreatedAt;
            Text = word.Text;
            Translation = word.Translation;
            Example = word.Example ?? string.Empty;

            switch (word)
            {
                case Noun noun:
                    SelectedKind = NounKind;
                    Article = WordFormatter.ArticleText(noun.Article);
                    Plural = noun.Plural ?? string.Empty;
                    break;
                case Verb verb:
                    SelectedKind = VerbKind;
                    Praeteritum = verb.Praeteritum ?? string.Empty;
                    PartizipII = verb.PartizipII ?? string.Empty;
                    Auxiliary = verb.Auxiliary == AuxiliaryVerb.Sein ? "sein" : "haben";
                    IsSeparable = verb.IsSeparable;
                    break;
                case Adjective adjective:
                    SelectedKind = AdjectiveKind;
                    Comparative = adjective.Comparative ?? string.Empty;
                    Superlative = adjective.Superlative ?? string.Empty;
                    break;
                case Phrase:
                    SelectedKind = PhraseKind;
                    break;
            }
        }

        private Word BuildWord()
        {
            Word word = SelectedKind switch
            {
                NounKind => new Noun
                {
                    Article = Article switch
                    {
                        "die" => Models.Article.Die,
                        "das" => Models.Article.Das,
                        _ => Models.Article.Der
                    },
                    Plural = Optional(Plural)
                },
                VerbKind => new Verb
                {
                    Praeteritum = Optional(Praeteritum),
                    PartizipII = Optional(PartizipII),
                    Auxiliary = Auxiliary == "sein" ? AuxiliaryVerb.Sein : AuxiliaryVerb.Haben,
                    IsSeparable = IsSeparable
                },
                AdjectiveKind => new Adjective
                {
                    Comparative = Optional(Comparative),
                    Superlative = Optional(Superlative)
                },
                _ => new Phrase()
            };

            word.Text = Text.Trim();
            word.Translation = Translation.Trim();
            word.Example = Optional(Example);

            return word;
        }

        private static string? Optional(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

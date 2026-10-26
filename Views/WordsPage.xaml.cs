using GermanyGlossary.ViewModels;

namespace GermanyGlossary.Views
{
    public partial class WordsPage : ContentPage
    {
        private readonly WordsViewModel _viewModel;

        public WordsPage(WordsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Reload every time so changes from the edit page show up.
            await _viewModel.LoadAsync();
        }
    }
}

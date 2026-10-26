using GermanyGlossary.ViewModels;

namespace GermanyGlossary.Views
{
    public partial class WordEditPage : ContentPage
    {
        private readonly WordEditViewModel _viewModel;

        public WordEditPage(WordEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }
    }
}

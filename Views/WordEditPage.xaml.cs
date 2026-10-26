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

        // Android hardware back: go through the same "unsaved changes?" check as the close button.
        protected override bool OnBackButtonPressed()
        {
            if (!_viewModel.IsDirty)
                return base.OnBackButtonPressed();

            _viewModel.CancelCommand.Execute(null);
            return true;
        }
    }
}

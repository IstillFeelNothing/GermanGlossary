using GermanyGlossary.ViewModels;

namespace GermanyGlossary.Views
{
    public partial class FlashcardsPage : ContentPage
    {
        private readonly FlashcardsViewModel _viewModel;
        private bool _isFlipping;

        public FlashcardsPage(FlashcardsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.EnsureStartedAsync();
        }

        // Flip animation is pure view logic, so it lives here; the state change goes through the ViewModel.
        private async void OnCardTapped(object? sender, TappedEventArgs e)
        {
            if (_isFlipping)
                return;

            _isFlipping = true;
            try
            {
                await Card.RotateYToAsync(90, 120, Easing.CubicIn);
                _viewModel.FlipCommand.Execute(null);
                Card.RotationY = -90;
                await Card.RotateYToAsync(0, 120, Easing.CubicOut);
            }
            finally
            {
                _isFlipping = false;
            }
        }
    }
}

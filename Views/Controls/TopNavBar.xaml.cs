namespace GermanyGlossary.Views.Controls
{
    public partial class TopNavBar : ContentView
    {
        // Shell route of the page this bar sits on; that item is drawn as selected.
        public static readonly BindableProperty CurrentRouteProperty = BindableProperty.Create(
            nameof(CurrentRoute), typeof(string), typeof(TopNavBar), AppShell.WordsRoute,
            propertyChanged: (bindable, _, _) => ((TopNavBar)bindable).UpdateSelection());

        public TopNavBar()
        {
            InitializeComponent();
            UpdateSelection();
        }

        public string CurrentRoute
        {
            get => (string)GetValue(CurrentRouteProperty);
            set => SetValue(CurrentRouteProperty, value);
        }

        private void UpdateSelection()
        {
            SetSelected(WordsTab, WordsLabel, CurrentRoute == AppShell.WordsRoute);
            SetSelected(CardsTab, CardsLabel, CurrentRoute == AppShell.CardsRoute);
        }

        private static void SetSelected(Border tab, Label label, bool isSelected)
        {
            tab.BackgroundColor = isSelected ? Resource("Primary") : Colors.Transparent;
            label.TextColor = isSelected ? Resource("TextPrimary") : Resource("TextSecondary");
        }

        private static Color Resource(string key) =>
            Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
                ? color
                : Colors.Transparent;

        private Task NavigateAsync(string route) =>
            route == CurrentRoute ? Task.CompletedTask : Shell.Current.GoToAsync($"//{route}");

        private async void OnWordsTapped(object? sender, TappedEventArgs e) => await NavigateAsync(AppShell.WordsRoute);

        private async void OnCardsTapped(object? sender, TappedEventArgs e) => await NavigateAsync(AppShell.CardsRoute);
    }
}

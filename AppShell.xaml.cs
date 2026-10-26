using GermanyGlossary.Views;

namespace GermanyGlossary
{
    public partial class AppShell : Shell
    {
        public const string WordsRoute = "words";
        public const string CardsRoute = "cards";

        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(WordEditPage), typeof(WordEditPage));
        }
    }
}

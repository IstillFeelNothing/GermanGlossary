using GermanyGlossary.Views;

namespace GermanyGlossary
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(WordEditPage), typeof(WordEditPage));
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace GermanyGlossary
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // The design is dark-only, so native controls (keyboards, popups, scrollbars) must match it.
            UserAppTheme = AppTheme.Dark;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
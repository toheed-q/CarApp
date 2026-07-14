namespace DMF
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // The whole app is designed dark-only (dark surfaces, white text), but the
            // styles use AppThemeBinding, so on a phone set to the Light system theme the
            // Light values won - backgrounds and popup/dialog windows rendered white.
            // Pinning the theme keeps the UI identical on every device regardless of the
            // user's system setting.
            UserAppTheme = AppTheme.Dark;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
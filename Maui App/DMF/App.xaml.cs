namespace DMF
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // NOTE: Pinning the theme here with `UserAppTheme = AppTheme.Dark` crashed
            // on launch (Material "valid TextAppearance" error), so the dark-only /
            // white-background fix is being redone at the native Android theme level
            // instead (Platforms/Android styles.xml). Do NOT re-add it here.
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
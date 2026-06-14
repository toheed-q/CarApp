namespace DMF.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel mainPage)
        {
            try
            {
                InitializeComponent();
                this.BindingContext = mainPage;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is MainPageModel vm)
            {
                if (vm.CurrentView == null)
                    vm.Initialize(); // 🔥 THIS MAKES HOME VIEW LOAD FIRST TIME

                // Singleton MainPage is reused across logins — refresh the cached
                // Account and Home views so they show the current user (and the
                // current user's wishlist hearts), not the previous one.
                _ = vm.RefreshAccountAsync();
                _ = vm.RefreshHomeAsync();
            }
        }
    }
}
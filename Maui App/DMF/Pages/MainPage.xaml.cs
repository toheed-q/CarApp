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

            if (BindingContext is MainPageModel vm && vm.CurrentView == null)
            {
                vm.Initialize(); // 🔥 THIS MAKES HOME VIEW LOAD FIRST TIME

            }
        }
    }
}
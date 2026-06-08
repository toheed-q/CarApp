namespace DMF.Pages
{
    public partial class EditProfilePage : ContentPage
    {
        public EditProfilePage(EditProfilePageModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is EditProfilePageModel vm)
                await vm.InitializeAsync();
        }
    }
}

namespace DMF.Pages;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(2000);
        CheckLoginDetails();
    }

    private void CheckLoginDetails()
    {
        if (BindingContext != null && BindingContext is SplashPageModel viewModel)
        {
            viewModel.NavigateToHomePage();
        }
    }
}
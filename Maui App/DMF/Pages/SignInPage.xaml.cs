namespace DMF.Pages;

public partial class SignInPage : ContentPage
{
    public SignInPage(SignInPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

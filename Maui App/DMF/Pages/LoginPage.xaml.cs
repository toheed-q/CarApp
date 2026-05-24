namespace DMF.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageModel pageModel)
    {
        try
        {
            InitializeComponent();
            this.BindingContext = pageModel;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
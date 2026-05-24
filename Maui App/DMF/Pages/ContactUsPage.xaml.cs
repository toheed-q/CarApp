namespace DMF.Pages;

public partial class ContactUsPage : ContentPage
{
    public ContactUsPage(ContactUsPageModel pageModel)
    {
        InitializeComponent();
        this.BindingContext = pageModel;
    }
}
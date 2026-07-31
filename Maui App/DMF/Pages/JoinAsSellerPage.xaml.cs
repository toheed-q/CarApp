namespace DMF.Pages;

public partial class JoinAsSellerPage : ContentPage
{
    public JoinAsSellerPage(JoinAsSellerPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

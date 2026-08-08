namespace DMF.Pages;

public partial class JoinAsSellerPage : ContentPage
{
    public JoinAsSellerPage(JoinAsSellerPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is JoinAsSellerPageModel vm)
            await vm.InitializeAsync();
    }
}

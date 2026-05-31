namespace DMF.Pages;

public partial class AccountView : ContentView
{
    private readonly AccountViewModel _vm;

    public AccountView(AccountViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler != null)
            await _vm.LoadUserAsync();
    }

    private async void AccountMenu_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is not VisualElement view) return;

        await TouchAnimation.AnimateAsync(view);

        if (view.BindingContext is AccountViewModel vm && e.Parameter is AccountMenuType menu)
        {
            switch (menu)
            {
                case AccountMenuType.Wishlist:      vm.ViewWishlistCommand.Execute(null); break;
                case AccountMenuType.JoinAsSeller:  vm.JoinAsSellerCommand.Execute(null); break;
                case AccountMenuType.ContactUs:     vm.ContactSupportCommand.Execute(null); break;
                case AccountMenuType.BuyPackages:   vm.BuyPackagesCommand.Execute(null); break;
                case AccountMenuType.ProfileView:   vm.ViewProfileCommand.Execute(null); break;
            }
        }
    }
}

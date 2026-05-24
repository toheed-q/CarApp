namespace DMF.Pages;

public partial class AccountView : ContentView
{
    public AccountView(AccountViewModel vm)
    {
        try
        {
            InitializeComponent();
            BindingContext = vm;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async void AccountMenu_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is not VisualElement view)
            return;

        await TouchAnimation.AnimateAsync(view);

        if (view.BindingContext is AccountViewModel vm)
        {
            if (e.Parameter is AccountMenuType menu)
            {
                switch (menu)
                {
                    case AccountMenuType.Wishlist:
                        vm.ViewWishlistCommand.Execute(null);
                        break;
                    case AccountMenuType.JoinAsSeller:
                        break;
                    case AccountMenuType.ContactUs:
                        vm.ContactSupportCommand.Execute(null);
                        break;
                    case AccountMenuType.BuyPackages:
                        break;
                    case AccountMenuType.ProfileView:
                        break;
                }
            }
        }
    }
}
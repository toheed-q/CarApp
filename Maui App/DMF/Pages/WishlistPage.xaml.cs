namespace DMF.Pages;

public partial class WishlistPage : ContentPage
{
    public WishlistPage(WishlistPageModel pageModel)
    {
        InitializeComponent();
        this.BindingContext = pageModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is WishlistPageModel vm && vm.CurrentView == null)
        {
            vm.Initialize();
        }
    }

    // NOTE: a manual slide-in animation used to live here (OnSizeAllocated set
    // Root.TranslationX = width, then TranslateTo 0). It ran on top of Shell's own
    // navigation transition, and the two collided — causing the clipping / overlap /
    // incomplete-render glitch when moving between Account and Wishlist. Removed so
    // Shell handles the transition cleanly.
}
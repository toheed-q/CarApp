namespace DMF.Pages;

public partial class WishlistPage : ContentPage
{
    private bool _hasAnimated;

    public WishlistPage(WishlistPageModel pageModel)
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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is WishlistPageModel vm && vm.CurrentView == null)
        {
            vm.Initialize();
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (_hasAnimated || width <= 0)
            return;

        _hasAnimated = true;

        // Start outside screen
        Root.TranslationX = width;

        _ = Root.TranslateTo(
            0,
            0,
            400,
            Easing.CubicOut);
    }
}
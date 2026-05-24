namespace DMF.Pages;

public partial class ProfileViewPage : ContentPage
{
    private bool _hasAnimated;
    public ProfileViewPage(ProfileViewPageModel vm)
    {
        try
        {
            InitializeComponent();
            this.BindingContext = vm;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private void ProfileViewRoot_Loaded(object sender, EventArgs e)
    {
        if (BindingContext is ProfileViewPageModel vm)
        {
            vm.Initialize();
        }
    }

    private async void CarDetailCommand_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is not VisualElement view)
            return;

        await TouchAnimation.AnimateAsync(view);

        //if (view.BindingContext is CarFilterResult car &&
        //    BindingContext is ProfileViewPageModel vm &&
        //    vm.CarDetailCommand.CanExecute(car))
        //{
        //    vm.CarDetailCommand.Execute(car);
        //}
    }
}
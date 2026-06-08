namespace DMF.Pages;

public partial class ProfileViewPage : ContentPage
{
    public ProfileViewPage(ProfileViewPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        // InitializeAsync is triggered via IQueryAttributable in the VM
        // so nothing needed here — VM handles both own and dealer profile
    }

    private void ProfileViewRoot_Loaded(object sender, EventArgs e)
    {
        // Do nothing here — initialization is driven by ApplyQueryAttributes
        // which fires after navigation params are set
    }
}

using DMF.Models;
using DMF.PageModels;

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

    // Tapping a portfolio card opens the same car detail page as the home screen.
    private void CarCard_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is BindableObject b && b.BindingContext is CarFilterResult car
            && BindingContext is ProfileViewPageModel vm)
            vm.OpenCarCommand.Execute(car);
    }

    // Invoking the VM command from code-behind avoids the unreliable
    // cross-template RelativeSource binding inside the CollectionView.
    private void EditButton_Clicked(object sender, EventArgs e)
    {
        if (sender is BindableObject b && b.BindingContext is CarFilterResult car
            && BindingContext is ProfileViewPageModel vm)
            vm.EditCarCommand.Execute(car);
    }

    private void DeleteButton_Clicked(object sender, EventArgs e)
    {
        if (sender is BindableObject b && b.BindingContext is CarFilterResult car
            && BindingContext is ProfileViewPageModel vm)
            vm.DeleteCarCommand.Execute(car);
    }
}

namespace DMF.Pages;

public partial class AddCarStep1Page : ContentPage
{
    public AddCarStep1Page(AddCarViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        Loaded += async (_, _) => await vm.InitializeAsync();
    }
}

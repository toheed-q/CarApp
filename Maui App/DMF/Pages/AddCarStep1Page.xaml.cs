namespace DMF.Pages;

[QueryProperty(nameof(EditCarId), "editCarId")]
public partial class AddCarStep1Page : ContentPage
{
    private readonly AddCarViewModel _vm;

    public AddCarStep1Page(AddCarViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
        Loaded += async (_, _) => await vm.InitializeAsync();
    }

    // Populated by Shell from the "editCarId" route parameter before the page loads.
    public int EditCarId
    {
        set => _vm.EditCarId = value;
    }
}

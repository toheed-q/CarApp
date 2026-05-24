namespace DMF.Pages;

[QueryProperty(nameof(Car), "Car")]
public partial class AddImageUploadPage : ContentPage
{
    private readonly ICarService _carService;

    public AddCarModel Car
    {
        set
        {
            BindingContext = new AddCarViewModel(_carService)
            {
                Car = value
            };
        }
    }

    public AddImageUploadPage(ICarService carService)
    {
        _carService = carService;
        InitializeComponent();
    }
}
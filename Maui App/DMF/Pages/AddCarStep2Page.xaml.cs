namespace DMF.Pages;

[QueryProperty(nameof(Car), "Car")]
public partial class AddCarStep2Page : ContentPage
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

    public AddCarStep2Page(ICarService carService)
    {
        _carService = carService;
        InitializeComponent();
    }
}
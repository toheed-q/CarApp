namespace DMF.Pages;

[QueryProperty(nameof(Car), "Car")]
public partial class AddCarStep3Page : ContentPage
{
    private readonly ICarService _carService;
    private readonly ICityService _cityService;
    private readonly ISecureStorageService _storage;

    public AddCarModel Car
    {
        set
        {
            BindingContext = new AddCarViewModel(_carService, _cityService, _storage)
            {
                Car = value
            };
        }
    }

    public AddCarStep3Page(ICarService carService, ICityService cityService, ISecureStorageService storage)
    {
        _carService = carService;
        _cityService = cityService;
        _storage = storage;
        InitializeComponent();
    }
}
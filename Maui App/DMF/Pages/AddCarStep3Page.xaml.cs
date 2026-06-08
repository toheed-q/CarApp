namespace DMF.Pages;

[QueryProperty(nameof(Car), "Car")]
public partial class AddCarStep3Page : ContentPage
{
    private readonly ICarService _carService;
    private readonly ISecureStorageService _storage;

    public AddCarModel Car
    {
        set
        {
            BindingContext = new AddCarViewModel(_carService, _storage)
            {
                Car = value
            };
        }
    }

    public AddCarStep3Page(ICarService carService, ISecureStorageService storage)
    {
        _carService = carService;
        _storage = storage;
        InitializeComponent();
    }
}
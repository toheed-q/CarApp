namespace DMF.Pages;

[QueryProperty(nameof(Car), "Car")]
public partial class AddImageUploadPage : ContentPage
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

    public AddImageUploadPage(ICarService carService, ISecureStorageService storage)
    {
        _carService = carService;
        _storage = storage;
        InitializeComponent();
    }
}
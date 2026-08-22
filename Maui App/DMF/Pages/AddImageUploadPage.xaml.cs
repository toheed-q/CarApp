namespace DMF.Pages;

[QueryProperty(nameof(Car), "Car")]
public partial class AddImageUploadPage : ContentPage
{
    private readonly ICarService _carService;
    private readonly ICityService _cityService;
    private readonly ISecureStorageService _storage;

    public AddCarModel Car
    {
        set
        {
            // Shell can re-apply this query attribute (e.g. after a popup closes).
            // Build the view model ONLY the first time — otherwise a fresh view model
            // is created and the user's picked photos get wiped and rebuilt from the
            // (empty) carried Car.Images. That was the "star clears all photos" bug.
            if (BindingContext is AddCarViewModel) return;

            BindingContext = new AddCarViewModel(_carService, _cityService, _storage)
            {
                Car = value
            };
        }
    }

    public AddImageUploadPage(ICarService carService, ICityService cityService, ISecureStorageService storage)
    {
        _carService = carService;
        _cityService = cityService;
        _storage = storage;
        InitializeComponent();
    }
}
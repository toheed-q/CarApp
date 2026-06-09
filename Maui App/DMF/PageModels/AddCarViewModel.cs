using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.Utilities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DMF.PageModels
{
    public partial class AddCarViewModel : ObservableObject
    {
        private readonly ICarService _carService;
        private readonly ICityService _cityService;
        private readonly ISecureStorageService _storage;

        [ObservableProperty] private AddCarModel car = new();
        [ObservableProperty] private bool isUploading = false;
        [ObservableProperty] private double uploadProgress = 0;
        [ObservableProperty] private bool isEditMode = false;

        // Display text for the city field; "Select City" until one is chosen.
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CityDisplay))]
        private string? selectedCityName;

        public string CityDisplay => string.IsNullOrWhiteSpace(SelectedCityName) ? "Select City" : SelectedCityName;

        // Set by AddCarStep1Page from the "editCarId" navigation parameter.
        public int EditCarId { get; set; }

        public string PageTitle => IsEditMode ? "Edit Car" : "Add a Car";
        partial void OnIsEditModeChanged(bool value) => OnPropertyChanged(nameof(PageTitle));

        private int _dealerId;
        private string _dealerName = "Dealer";

        public ObservableCollection<ImageItem> Images { get; set; } = new();
        public List<string> YesNoOptions { get; } = ["Yes", "No"];

        public ICommand BrowseCommand { get; }
        public ICommand RemoveCommand { get; }

        public AddCarViewModel(ICarService carService, ICityService cityService, ISecureStorageService storage)
        {
            BrowseCommand = new Command(async () => await PickImageAsync());
            RemoveCommand = new Command<ImageItem>(RemoveImage);
            _carService = carService;
            _cityService = cityService;
            _storage = storage;
        }

        public async Task InitializeAsync()
        {
            var idStr = await _storage.GetAsync(AppConstants.DealersId);
            int.TryParse(idStr, out _dealerId);
            _dealerName = await _storage.GetAsync(AppConstants.UserName) ?? "Dealer";

            if (EditCarId > 0)
            {
                await LoadCarForEditAsync(EditCarId);
                return;
            }

            Car.DealersID = _dealerId;
        }

        // Fetches the existing car and maps it into the wizard so every field is pre-filled.
        private async Task LoadCarForEditAsync(int carId)
        {
            IsEditMode = true;

            var response = await _carService.GetCarByIdAsync(carId);
            var c = response?.Data;
            if (c == null) return;

            Car = new AddCarModel
            {
                ID                = c.ID,
                DealersID         = c.DealersID > 0 ? c.DealersID : _dealerId,
                Brand             = c.Brand ?? string.Empty,
                Model             = c.Model ?? string.Empty,
                YearOfManufacture = c.RegistrationDate?.Year,
                RegistrationNo    = c.RegistrationNo ?? string.Empty,
                PurchaseDate      = c.RegistrationDate,
                FuelType          = c.Fuel ?? string.Empty,
                Transmission      = c.Transmission ?? string.Empty,
                OdometerReading   = c.KMDriven,
                AccidentHistory   = c.IsAccidental,
                ServiceHistory    = c.ServiceHistory,
                Price             = c.Price,
                AlloyWheels       = c.AlloyWheels,
                Bluetooth         = c.Bluetooth,
                PowerSteering     = c.PowerStaring,
                PowerWindow       = c.PowerWindow,
                Airbags           = c.AirBag,
                ABS               = c.ABS,
                AirCondition      = c.AirCondition == "Yes" ? true
                                  : c.AirCondition == "No" ? false
                                  : (bool?)null,
                CityId            = c.CityId
            };

            // Resolve the city name for display (server returns only CityId).
            if (Car.CityId is int cid && cid > 0)
            {
                try
                {
                    var citiesResp = await _cityService.GetActiveCitiesAsync();
                    var match = citiesResp?.Data?.FirstOrDefault(x => x.Id == cid);
                    if (match != null)
                    {
                        Car.CityName = match.CityName;
                        SelectedCityName = match.CityName;
                    }
                }
                catch { /* name is cosmetic; CityId is already preserved */ }
            }
        }

        // Opens the searchable city picker (same popup as the search bar) and
        // stores the chosen city on the model. City is required to continue.
        [RelayCommand]
        async Task SelectCity()
        {
            var popup = new DMF.Pages.Popups.CitySelectionPopup(_cityService, allowAllLocations: false);

            var result = await Application.Current!.Windows[0].Page!
                .ShowPopupAsync(popup) as DMF.Pages.Popups.CitySelectionResult;

            if (result?.City == null) return; // cancelled — keep previous selection

            Car.CityId = result.City.Id;
            Car.CityName = result.City.CityName;
            SelectedCityName = result.City.CityName;
        }

        [RelayCommand]
        async Task NextStep1()
        {
            if (Car.CityId is null or <= 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "City required", "Please select the city this car is listed in.", "OK");
                return;
            }

            await Shell.Current.GoToAsync("AddCarStep2", true,
                new Dictionary<string, object> { { "Car", Car } });
        }

        [RelayCommand]
        async Task NextStep2()
        {
            await Shell.Current.GoToAsync("AddCarStep3", true,
                new Dictionary<string, object> { { "Car", Car } });
        }

        [RelayCommand]
        async Task NextStep3()
        {
            await Shell.Current.GoToAsync("AddCarStep4", true,
                new Dictionary<string, object> { { "Car", Car } });
        }

        [RelayCommand]
        async Task Submit()
        {
            try
            {
                // Backstop: city is mandatory (also enforced at step 1).
                if (Car.CityId is null or <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "City required", "Please go back to step 1 and select a city.", "OK");
                    return;
                }

                IsUploading = true;
                UploadProgress = 0;

                await CaptureLocationAsync();

                bool isEdit = Car.ID is > 0;

                Func<double, Task> onProgress = async progress =>
                {
                    MainThread.BeginInvokeOnMainThread(() => UploadProgress = progress);
                };

                var result = isEdit
                    ? await _carService.UpdateCarAsync(Car, Images, _dealerName, _dealerId, onProgress)
                    : await _carService.AddCarAsync(Car, Images, _dealerName, _dealerId, onProgress);

                IsUploading = false;

                if (result.Success)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", isEdit ? "Car Updated" : "Car Added", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                IsUploading = false;
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        [RelayCommand] Task Back() => Shell.Current.GoToAsync("..", true);

        private async Task CaptureLocationAsync()
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted) return;

                Location? location = null;
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    location = await Geolocation.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(10)), cts.Token);
                }
                catch { }

                location ??= await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    Car.Latitude  = location.Latitude;
                    Car.Longitude = location.Longitude;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Location] failed: {ex.Message}");
            }
        }

        [RelayCommand]
        public async void NavigateToHome() => await Shell.Current.GoToAsync("///mainPage");

        private async Task PickImageAsync()
        {
            try
            {
                if (Images.Count >= 20)
                {
                    await Application.Current.MainPage.DisplayAlert("Limit", "Maximum 20 images allowed", "OK");
                    return;
                }

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Image",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                    Images.Add(new ImageItem { FilePath = result.FullPath });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void RemoveImage(ImageItem item)
        {
            if (item != null) Images.Remove(item);
        }
    }
}

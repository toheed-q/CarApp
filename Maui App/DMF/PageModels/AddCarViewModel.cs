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
        public ICommand SetPrimaryCommand { get; }

        public AddCarViewModel(ICarService carService, ICityService cityService, ISecureStorageService storage)
        {
            BrowseCommand = new Command(async () => await PickImageAsync());
            RemoveCommand = new Command<ImageItem>(RemoveImage);
            SetPrimaryCommand = new Command<ImageItem>(async item => await SetPrimaryAsync(item));
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

        // Each wizard page builds its own view model and only the Car is carried
        // across navigation. Whenever a Car is assigned, rebuild the photo picker
        // from its carried URLs so already-uploaded photos appear on the picture
        // step (and mark the first as primary).
        partial void OnCarChanged(AddCarModel value)
        {
            Images.Clear();
            if (value?.Images != null)
            {
                foreach (var url in value.Images.Where(u => !string.IsNullOrWhiteSpace(u)))
                    Images.Add(new ImageItem { FilePath = url, IsExisting = true });
            }
            EnsurePrimary();
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
                BodyType          = c.BodyType ?? string.Empty,
                IsNegotiable      = c.IsNegotiable,
                ReverseCamera     = c.ReverseCamera,
                Sunroof           = c.Sunroof,
                CityId            = c.CityId,
                // Carry the already-uploaded photo URLs so they survive navigation
                // to the picture step (each wizard page builds its own view model).
                Images            = c.Images?.Images?
                                        .Where(u => !string.IsNullOrWhiteSpace(u))
                                        .ToList() ?? new()
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
                await ShowMessageAsync(PopupType.Warning,
                    "City required", "Please select the city this car is listed in.");
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
                    await ShowMessageAsync(PopupType.Warning,
                        "City required", "Please go back to step 1 and select a city.");
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
                    await ShowMessageAsync(PopupType.Success, "Success",
                        isEdit ? "Your car was updated successfully." : "Your car was added successfully.");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await ShowMessageAsync(PopupType.Error, "Something went wrong", result.Message);
                }
            }
            catch (Exception ex)
            {
                IsUploading = false;
                await ShowMessageAsync(PopupType.Error, "Something went wrong", ex.Message);
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
                    await ShowMessageAsync(PopupType.Warning, "Limit reached", "You can add a maximum of 20 images.");
                    return;
                }

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Image",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    Images.Add(new ImageItem { FilePath = result.FullPath });
                    EnsurePrimary();
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync(PopupType.Error, "Something went wrong", ex.Message);
            }
        }

        private void RemoveImage(ImageItem item)
        {
            if (item == null) return;
            Images.Remove(item);
            EnsurePrimary();
        }

        // Marks the tapped photo as the primary (listing thumbnail) and clears the
        // flag on every other photo, so exactly one is ever primary. Confirms to
        // the user that the listing photo was changed.
        private async Task SetPrimaryAsync(ImageItem item)
        {
            if (item == null || item.IsPrimary) return;

            foreach (var img in Images)
                img.IsPrimary = ReferenceEquals(img, item);

            await ShowMessageAsync(PopupType.Success, "Primary photo",
                "This picture is set as the primary photo for the listing.");
        }

        // Guarantees there is always exactly one primary photo: if none is marked
        // (first add, or the primary was removed), the first photo becomes primary.
        private void EnsurePrimary()
        {
            if (Images.Count == 0) return;
            if (!Images.Any(i => i.IsPrimary))
                Images[0].IsPrimary = true;
        }

        // Shows the styled, on-brand popup and awaits until the user dismisses it.
        private static Task ShowMessageAsync(PopupType type, string title, string? message)
        {
            var popup = new DMF.Pages.Popups.CustomPopup(
                new PopupModel { PopupType = type, PopupName = title, PopupMessage = message }, null);

            return Application.Current!.Windows[0].Page!.ShowPopupAsync(popup);
        }
    }
}

using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Extensions;
using DMF.Pages.Popups;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.Services.Interfaces;
using DMF.Utilities;
using Microsoft.Extensions.DependencyInjection;
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

        // Brand / Model dropdowns. Brand is chosen from the lookup table; Model is
        // filtered to the chosen brand. Variant stays free-text on the model itself.
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BrandDisplay))]
        [NotifyPropertyChangedFor(nameof(HasBrand))]
        private string? selectedBrand;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ModelDisplay))]
        private string? selectedModel;

        public string BrandDisplay => string.IsNullOrWhiteSpace(SelectedBrand) ? "Select Brand" : SelectedBrand!;
        public string ModelDisplay => string.IsNullOrWhiteSpace(SelectedModel) ? "Select Model" : SelectedModel!;
        public bool HasBrand => !string.IsNullOrWhiteSpace(SelectedBrand);

        // Lookup caches: all brands (A–Z) and the models of the currently chosen brand.
        private List<string> _brands = new();
        private List<string> _modelsForBrand = new();

        // Set by AddCarStep1Page from the "editCarId" navigation parameter.
        public int EditCarId { get; set; }

        public string PageTitle => IsEditMode ? "Edit Car" : "Add a Car";
        partial void OnIsEditModeChanged(bool value) => OnPropertyChanged(nameof(PageTitle));

        private int _dealerId;
        private string _dealerName = "Dealer";

        public ObservableCollection<ImageItem> Images { get; set; } = new();
        public List<string> YesNoOptions { get; } = ["Yes", "No"];

        // Fixed dropdown options for the Basic Details step. The saved value is one of
        // these strings, so on edit the picker pre-selects the previously chosen option.
        public List<string> FuelTypes { get; } = ["CNG", "Petrol", "Diesel", "Hybrid", "Electric"];
        public List<string> Transmissions { get; } = ["Auto", "Manual", "CVT"];
        public List<string> BodyTypes { get; } = ["Sedan", "SUV", "Hatchback", "Crossover", "Coupe", "Van", "Pickup"];

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

            // Preload the brand list so the Brand dropdown opens instantly.
            _brands = await _carService.GetBrandsAsync() ?? new();

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

            // Reflect the carried Brand/Model into the dropdown display (a fresh view
            // model is built for each wizard page, so this keeps Step 1 in sync).
            SelectedBrand = string.IsNullOrWhiteSpace(value?.Brand) ? null : value.Brand;
            SelectedModel = string.IsNullOrWhiteSpace(value?.Model) ? null : value.Model;
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
                Varient           = c.Varient ?? string.Empty,
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

            await Application.Current!.Windows[0].Page!
                .ShowPopupAsync(popup, PopupDefaults.Sheet());
            var result = popup.SelectionResult;

            if (result?.City == null) return; // cancelled — keep previous selection

            Car.CityId = result.City.Id;
            Car.CityName = result.City.CityName;
            SelectedCityName = result.City.CityName;
        }

        // Brand picker (searchable, A–Z). Choosing a brand clears any previous model
        // and loads that brand's models for the dependent Model dropdown.
        [RelayCommand]
        async Task SelectBrand()
        {
            if (_brands.Count == 0)
                _brands = await _carService.GetBrandsAsync() ?? new();

            var popup = new DMF.Pages.Popups.SearchableSelectPopup(_brands, "Brand", "Search brand...");
            await Application.Current!.Windows[0].Page!.ShowPopupAsync(popup, PopupDefaults.Sheet());
            var result = popup.SelectedValue;

            if (string.IsNullOrWhiteSpace(result) || result == SelectedBrand) return;

            SelectedBrand = result;
            Car.Brand = result;

            // Brand changed → reset the model and refresh its option list.
            SelectedModel = null;
            Car.Model = string.Empty;
            _modelsForBrand = await _carService.GetModelsByBrandAsync(result) ?? new();
        }

        // Model picker (searchable) — only the selected brand's models.
        [RelayCommand]
        async Task SelectModel()
        {
            if (string.IsNullOrWhiteSpace(SelectedBrand))
            {
                await ShowMessageAsync(PopupType.Warning,
                    "Select a brand first", "Please choose a brand before selecting a model.");
                return;
            }

            if (_modelsForBrand.Count == 0)
                _modelsForBrand = await _carService.GetModelsByBrandAsync(SelectedBrand) ?? new();

            var popup = new DMF.Pages.Popups.SearchableSelectPopup(_modelsForBrand, "Model", "Search model...");
            await Application.Current!.Windows[0].Page!.ShowPopupAsync(popup, PopupDefaults.Sheet());
            var result = popup.SelectedValue;

            if (string.IsNullOrWhiteSpace(result)) return;

            SelectedModel = result;
            Car.Model = result;
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

                    // After OK, land the dealer on their own profile so they immediately
                    // see the listing. Reset to home first so the Add-Car wizard is cleared
                    // from the back stack (Back from the profile returns to Home, not here).
                    MainPageModel.ForceHomeOnAppear = true;
                    await Shell.Current.GoToAsync("///mainPage");
                    await Shell.Current.GoToAsync("profile");
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
        public async void NavigateToHome()
        {
            MainPageModel.ForceHomeOnAppear = true;
            await Shell.Current.GoToAsync("///mainPage");
        }

        private async Task PickImageAsync()
        {
            try
            {
                int remaining = 20 - Images.Count;
                if (remaining <= 0)
                {
                    await ShowMessageAsync(PopupType.Warning, "Limit reached", "You can add a maximum of 20 images.");
                    return;
                }

                var page = Application.Current?.Windows[0].Page;
                if (page is null) return;

                // Ask the source first — dark sheet, consistent with the app (no search).
                var sourcePopup = new DMF.Pages.Popups.SearchableSelectPopup(
                    new[] { "Photo Library", "Camera" }, "Add Photos",
                    showSearch: false, sort: false);
                await page.ShowPopupAsync(sourcePopup, PopupDefaults.Sheet());
                var choice = sourcePopup.SelectedValue;
                if (string.IsNullOrEmpty(choice))
                    return;

                var newPaths = new List<string>();

                if (choice == "Camera")
                {
                    if (MediaPicker.Default.IsCaptureSupported)
                    {
                        var photo = await MediaPicker.Default.CapturePhotoAsync();
                        if (photo is not null)
                            newPaths.Add(photo.FullPath);
                    }
                }
                else
                {
                    // Photo Library — native gallery, multi-select (the OS shows the
                    // 1, 2, 3… selection order), not the file explorer.
                    var picker = IPlatformApplication.Current?.Services?.GetService<IPhotoPicker>();
                    if (picker is not null)
                        newPaths.AddRange(await picker.PickImagesAsync(remaining));
                }

                if (newPaths.Count == 0)
                    return;

                // Honour the 20-image cap: add what fits, tell the user if some were skipped.
                var toAdd = newPaths.Take(remaining).ToList();
                foreach (var path in toAdd)
                    Images.Add(new ImageItem { FilePath = path });

                EnsurePrimary();

                if (newPaths.Count > toAdd.Count)
                    await ShowMessageAsync(PopupType.Warning, "Limit reached",
                        $"Added {toAdd.Count} photo(s). A listing can have at most 20 images.");
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

            return Application.Current!.Windows[0].Page!.ShowPopupAsync(popup, PopupDefaults.Sheet());
        }
    }
}

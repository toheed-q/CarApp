using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DMF.PageModels
{
    public partial class AddCarViewModel : ObservableObject
    {
        private readonly ICarService _carService;

        [ObservableProperty]
        private AddCarModel car = new();

        [ObservableProperty]
        private bool isUploading = false;

        [ObservableProperty]
        private double uploadProgress = 0;


        public ObservableCollection<ImageItem> Images { get; set; } = new();

        public List<string> YesNoOptions { get; } = ["Yes", "No"];

        public ICommand BrowseCommand { get; }
        public ICommand RemoveCommand { get; }

        public AddCarViewModel(ICarService carService)
        {
            BrowseCommand = new Command(async () => await PickImageAsync());
            RemoveCommand = new Command<ImageItem>(RemoveImage);
            _carService = carService;
        }

        [RelayCommand]
        async Task NextStep1()
        {
            await Shell.Current.GoToAsync("AddCarStep2", true,
                new Dictionary<string, object>
                {
                    { "Car", Car }
                });
        }

        [RelayCommand]
        async Task NextStep2()
        {
            await Shell.Current.GoToAsync("AddCarStep3", true,
                new Dictionary<string, object>
                {
                    { "Car", Car }
                });
        }

        [RelayCommand]
        async Task NextStep3()
        {
            await Shell.Current.GoToAsync("AddCarStep4", true,
                new Dictionary<string, object>
                {
                    { "Car", Car }
                });
        }

        [RelayCommand]
        async Task Submit()
        {
            try
            {
                IsUploading = true;
                UploadProgress = 0;

                await CaptureLocationAsync();

                var result = await _carService.AddCarAsync(
                    Car,
                    Images,
                    "MyDealer",   // replace dynamically
                    101,          // dealerId
                    async progress =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            UploadProgress = progress;
                        });
                    });

                IsUploading = false;

                if (result.Success)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Car Added", "OK");
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
                System.Diagnostics.Debug.WriteLine($"[Location] Permission: {status}");
                if (status != PermissionStatus.Granted)
                    return;

                Location? location = null;

                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    location = await Geolocation.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(10)), cts.Token);
                    System.Diagnostics.Debug.WriteLine($"[Location] Live fix: {location?.Latitude}, {location?.Longitude}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Location] Live fix failed: {ex.Message}");
                }

                location ??= await Geolocation.GetLastKnownLocationAsync();
                System.Diagnostics.Debug.WriteLine($"[Location] After fallback: {location?.Latitude}, {location?.Longitude}");

                if (location != null)
                {
                    Car.Latitude  = location.Latitude;
                    Car.Longitude = location.Longitude;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[Location] WARNING: No location obtained — CarLocation will be NULL in DB");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Location] CaptureLocationAsync failed: {ex.Message}");
            }
        }

        [RelayCommand]
        public async void NavigateToHome()
        {
            await Shell.Current.GoToAsync("///mainPage");
        }

        #region Car Image Upload



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
                {
                    Images.Add(new ImageItem
                    {
                        FilePath = result.FullPath
                    });
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void RemoveImage(ImageItem item)
        {
            if (item == null) return;

            Images.Remove(item);
        }

        #endregion
    }
}

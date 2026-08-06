using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.Utilities;

namespace DMF.PageModels
{
    public partial class CarDetailPageModel : ObservableObject, IQueryAttributable
    {
        private readonly IUserDetailService _userDetailService;
        private readonly ICarService _carService;
        private readonly ISecureStorageService _storage;

        [ObservableProperty]
        private CarFilterResult carDetail;

        [ObservableProperty]
        private int currentImageIndex;

        [ObservableProperty]
        private bool isFavorite;

        // Seller's profile photo (URL or placeholder token), resolved by the converter.
        [ObservableProperty]
        private string? sellerImage;

        // Seller's actual phone number — dialled by the Call button.
        [ObservableProperty]
        private string? sellerMobile;

        public string ImageCounter =>
            $"{CurrentImageIndex + 1}/{CarDetail?.Images?.Count ?? 1}";

        partial void OnCurrentImageIndexChanged(int value) =>
            OnPropertyChanged(nameof(ImageCounter));

        public CarDetailPageModel(IUserDetailService userDetailService, ICarService carService, ISecureStorageService storage)
        {
            _userDetailService = userDetailService;
            _carService = carService;
            _storage = storage;
            carDetail = new CarFilterResult();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("carDetail", out var car))
            {
                CarDetail = (CarFilterResult)car ?? new CarFilterResult();
                // Reflect the car's real wishlist state on the heart.
                IsFavorite = CarDetail?.IsWishlisted ?? false;
                OnPropertyChanged(nameof(ImageCounter));
                _ = LoadDealerNameAsync();
            }
        }

        private async Task LoadDealerNameAsync()
        {
            if (CarDetail?.DealersID == null) return;

            var result = await _userDetailService.GetByIdAsync(CarDetail.DealersID.Value);
            if (result?.Data != null)
            {
                CarDetail.DealerName = result.Data.CompanyName ?? result.Data.FirstName;
                SellerImage = result.Data.ProfileImage;
                SellerMobile = result.Data.PrimaryMobile;
                OnPropertyChanged(nameof(CarDetail));
            }
        }

        [RelayCommand] Task Back() => Shell.Current.GoToAsync("..");

        [RelayCommand]
        private async Task Share()
        {
            var id = CarDetail?.ID ?? 0;
            if (id <= 0) return;

            // Public share URL — the Netlify landing page deep-links into the app when
            // installed, otherwise sends the recipient to the Play Store.
            var url = $"{ApiConstants.ShareBaseUrl}?id={id}";

            var name = string.IsNullOrWhiteSpace(CarDetail?.Name) ? "this car" : CarDetail!.Name;

            await Microsoft.Maui.ApplicationModel.DataTransfer.Share.Default.RequestAsync(
                new Microsoft.Maui.ApplicationModel.DataTransfer.ShareTextRequest
                {
                    Uri = url,
                    Text = $"Check out {name} on Car Deal:\n{url}",
                    Title = "Share this car"
                });
        }

        [RelayCommand]
        private async Task Favorite()
        {
            var carId = CarDetail?.ID ?? 0;
            if (carId <= 0) return;

            var idStr = await _storage.GetAsync(AppConstants.UserId);
            if (!int.TryParse(idStr, out var userId) || userId <= 0)
            {
                await Application.Current!.Windows[0].Page!.DisplayAlert(
                    "Sign in required", "Please sign in to save cars to your wishlist.", "OK");
                return;
            }

            var response = await _carService.ToggleWishlistAsync(userId, carId);
            if (response.Success)
            {
                IsFavorite = response.Data;
                // Update the shared model so Home / Favorites reflect it on return.
                if (CarDetail is not null)
                    CarDetail.IsWishlisted = response.Data;
            }
        }

        [RelayCommand]
        private void NextImage()
        {
            if (CurrentImageIndex < (CarDetail?.Images.Count ?? 1) - 1)
                CurrentImageIndex++;
        }

        [RelayCommand]
        private void PreviousImage()
        {
            if (CurrentImageIndex > 0)
                CurrentImageIndex--;
        }

        [RelayCommand]
        private void ViewUserProfile()
        {
            if (CarDetail?.DealersID == null) return;

            Shell.Current.GoToAsync("profile", new Dictionary<string, object>
            {
                { "dealerId", CarDetail.DealersID.Value.ToString() }
            });
        }

        // Dials the seller's actual phone number (not the car's registration no).
        [RelayCommand]
        private async Task CallSeller()
        {
            var number = SellerMobile?.Trim();
            if (string.IsNullOrWhiteSpace(number))
                return;

            try { await Launcher.Default.OpenAsync($"tel:{number}"); }
            catch { /* no dialer available */ }
        }
    }
}

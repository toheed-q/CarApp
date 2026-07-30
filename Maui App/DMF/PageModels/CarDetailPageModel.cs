using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMF.PageModels
{
    public partial class CarDetailPageModel : ObservableObject, IQueryAttributable
    {
        private readonly IUserDetailService _userDetailService;

        [ObservableProperty]
        private CarFilterResult carDetail;

        [ObservableProperty]
        private int currentImageIndex;

        [ObservableProperty]
        private bool isFavorite;

        // Seller's profile photo (URL or placeholder token), resolved by the converter.
        [ObservableProperty]
        private string? sellerImage;

        public string ImageCounter =>
            $"{CurrentImageIndex + 1}/{CarDetail?.Images?.Count ?? 1}";

        partial void OnCurrentImageIndexChanged(int value) =>
            OnPropertyChanged(nameof(ImageCounter));

        public CarDetailPageModel(IUserDetailService userDetailService)
        {
            _userDetailService = userDetailService;
            carDetail = new CarFilterResult();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("carDetail", out var car))
            {
                CarDetail = (CarFilterResult)car ?? new CarFilterResult();
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
                    Text = $"Check out {name} on DMF Motors:\n{url}",
                    Title = "Share this car"
                });
        }

        [RelayCommand]
        private void Favorite() => IsFavorite = !IsFavorite;

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

        [RelayCommand]
        private async Task VerifyOtp()
        {
            if (!string.IsNullOrWhiteSpace(CarDetail?.DealerName))
                await Launcher.OpenAsync($"tel:{CarDetail?.RegistrationNo}");
        }
    }
}

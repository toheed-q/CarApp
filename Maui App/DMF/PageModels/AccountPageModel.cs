using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.Utilities;

namespace DMF.PageModels
{
    public partial class AccountViewModel : ObservableObject
    {
        private readonly ISecureStorageService _storage;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string userName = string.Empty;

        [ObservableProperty]
        private string userMobile = string.Empty;

        public AccountViewModel(ISecureStorageService storage, IAuthService authService)
        {
            _storage = storage;
            _authService = authService;
        }

        public async Task LoadUserAsync()
        {
            UserName = await _storage.GetAsync(AppConstants.UserName) ?? "Guest";
            UserMobile = await _storage.GetAsync(AppConstants.UserMobile) ?? string.Empty;
        }

        [RelayCommand]
        async Task Logout()
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("///login");
        }

        [RelayCommand]
        void ViewProfile() => Shell.Current.GoToAsync("profile");

        [RelayCommand]
        void ContactSupport() => Shell.Current.GoToAsync("contactus");

        [RelayCommand]
        void BuyPackages() { }

        [RelayCommand]
        void JoinAsSeller() { }

        [RelayCommand]
        void ViewWishlist() => Shell.Current.GoToAsync("wishlist");
    }
}

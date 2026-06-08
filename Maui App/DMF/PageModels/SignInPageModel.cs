using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.Constants;
using DMF.DTOs.Auth;
using DMF.Utilities;

namespace DMF.PageModels
{
    public partial class SignInPageModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IUserDetailService _userDetailService;
        private readonly ISecureStorageService _storage;
        private readonly IPopupService _popupService;

        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private bool isBusy = false;
        [ObservableProperty] private bool isPasswordVisible = false;

        public string PasswordToggleIcon => IsPasswordVisible ? "eye_off" : "eye";

        partial void OnIsPasswordVisibleChanged(bool value) =>
            OnPropertyChanged(nameof(PasswordToggleIcon));

        public SignInPageModel(
            IAuthService authService,
            IUserDetailService userDetailService,
            ISecureStorageService storage,
            IPopupService popupService)
        {
            _authService = authService;
            _userDetailService = userDetailService;
            _storage = storage;
            _popupService = popupService;
        }

        [RelayCommand]
        private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

        [RelayCommand]
        private async Task SignIn()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await _popupService.ShowPopupAsync(new PopupModel
                {
                    PopupName = "Validation Error",
                    PopupMessage = "• Please enter your email and password."
                });
                return;
            }

            IsBusy = true;
            try
            {
                var response = await _authService.LoginWithEmailAsync(new EmailLoginRequestDto
                {
                    Email = Email,
                    Password = Password
                });

                if (!response.Success || response.Data == null)
                {
                    await _popupService.ShowPopupAsync(new PopupModel
                    {
                        PopupName = "Login Failed",
                        PopupMessage = response.Message ?? "Invalid email or password."
                    });
                    return;
                }

                // Fetch user details by email to populate secure storage
                var userResponse = await _userDetailService.GetByEmailAsync(Email);
                var user = userResponse?.Data;

                if (user != null)
                {
                    await _storage.SetAsync(AppKeys.AuthToken, response.Data.Token);
                    await _storage.SetAsync(AppConstants.UserId, user.ID.ToString());
                    await _storage.SetAsync(AppConstants.DealersId, user.ID.ToString());
                    await _storage.SetAsync(AppConstants.UserName, user.CompanyName ?? user.FirstName);
                    await _storage.SetAsync(AppConstants.UserMobile, user.PrimaryMobile);
                    await _storage.SetAsync(AppConstants.IsDealers, user.IsDealers.ToString());

                    if (!string.IsNullOrWhiteSpace(user.City))
                        await _storage.SetAsync(AppConstants.UserCity, user.City);

                    if (!string.IsNullOrWhiteSpace(user.Email))
                        await _storage.SetAsync(AppConstants.UserEmail, user.Email);
                }

                await Shell.Current.GoToAsync("///mainPage");
            }
            catch (Exception)
            {
                await _popupService.ShowPopupAsync(new PopupModel
                {
                    PopupName = "Connection Error",
                    PopupMessage = "• Could not reach the server. Please check your connection."
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private Task NavigateToRegister() => Shell.Current.GoToAsync("///login");

        [RelayCommand]
        private Task Back() => Shell.Current.GoToAsync("..");
    }
}

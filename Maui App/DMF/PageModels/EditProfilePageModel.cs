using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.Constants;
using DMF.DTOs.Auth;
using DMF.DTOs.User;
using DMF.Utilities;

namespace DMF.PageModels
{
    public partial class EditProfilePageModel : ObservableObject
    {
        private readonly IUserDetailService _userDetailService;
        private readonly IAuthService _authService;
        private readonly ISecureStorageService _storage;

        private int _userId;

        [ObservableProperty] private string fullName = string.Empty;
        [ObservableProperty] private string primaryMobile = string.Empty;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? address1;
        [ObservableProperty] private string? state;
        [ObservableProperty] private string? city;
        [ObservableProperty] private string? pincode;
        [ObservableProperty] private string newPassword = string.Empty;
        [ObservableProperty] private string confirmPassword = string.Empty;
        [ObservableProperty] private bool isPasswordVisible = false;

        // kept for save payload
        private string? _lastName;
        private string? _companyName;
        private string? _secondaryMobile;
        private string? _address2;
        private string? _district;
        private string _profileImage = "default.png";

        public EditProfilePageModel(
            IUserDetailService userDetailService,
            IAuthService authService,
            ISecureStorageService storage)
        {
            _userDetailService = userDetailService;
            _authService = authService;
            _storage = storage;
        }

        public async Task InitializeAsync()
        {
            var idStr = await _storage.GetAsync(AppConstants.UserId);
            int.TryParse(idStr, out _userId);

            var result = await _userDetailService.GetByIdAsync(_userId);
            if (result?.Data == null) return;

            var u = result.Data;
            FullName        = string.Join(" ", new[] { u.FirstName, u.LastName }.Where(s => !string.IsNullOrEmpty(s)));
            PrimaryMobile   = u.PrimaryMobile;
            Email           = u.Email;
            Address1        = u.Address1;
            State           = u.State;
            City            = u.City;
            Pincode         = u.Pincode;

            // preserve fields not on this form
            _lastName        = u.LastName;
            _companyName     = u.CompanyName;
            _secondaryMobile = u.SecondaryMobile;
            _address2        = u.Address2;
            _district        = u.District;
            _profileImage    = u.ProfileImage ?? "default.png";
        }

        [RelayCommand]
        private async Task Save()
        {
            // Validate password if provided
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (NewPassword.Length < 6)
                {
                    await Shell.Current.CurrentPage.DisplayAlert(
                        "Validation", "Password must be at least 6 characters.", "OK");
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    await Shell.Current.CurrentPage.DisplayAlert(
                        "Validation", "Passwords do not match.", "OK");
                    return;
                }
            }

            // Split FullName back into first / last
            var parts = (FullName ?? string.Empty).Trim().Split(' ', 2);
            var firstName = parts.ElementAtOrDefault(0) ?? string.Empty;
            var lastName  = parts.ElementAtOrDefault(1) ?? _lastName;

            var dto = new UserDetailDto
            {
                FirstName       = firstName,
                LastName        = lastName,
                CompanyName     = _companyName,
                PrimaryMobile   = PrimaryMobile,
                SecondaryMobile = _secondaryMobile,
                Email           = Email,
                Address1        = Address1,
                Address2        = _address2,
                State           = State,
                District        = _district,
                City            = City,
                Pincode         = Pincode,
                ProfileImage    = _profileImage,
            };

            var result = await _userDetailService.UpdateAsync(_userId, dto);

            if (!result.Success)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Error", result.Message ?? "Update failed", "OK");
                return;
            }

            // Save password if provided
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                var pwResult = await _authService.SetPasswordAsync(new SetPasswordRequestDto
                {
                    UserId   = _userId,
                    Password = NewPassword
                });

                if (!pwResult.Success)
                {
                    await Shell.Current.CurrentPage.DisplayAlert(
                        "Warning", "Profile saved but password could not be set: " + pwResult.Message, "OK");
                }
            }

            // Persist updated values locally
            if (!string.IsNullOrWhiteSpace(City))
                await _storage.SetAsync(AppConstants.UserCity, City);

            if (!string.IsNullOrWhiteSpace(FullName))
                await _storage.SetAsync(AppConstants.UserName, FullName.Trim());

            if (!string.IsNullOrWhiteSpace(Email))
                await _storage.SetAsync(AppConstants.UserEmail, Email);

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private Task Back() => Shell.Current.GoToAsync("..");
    }
}

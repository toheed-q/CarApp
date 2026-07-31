using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.Constants;
using DMF.DTOs.Auth;
using DMF.DTOs.User;
using DMF.Helpers;
using DMF.Services.Interfaces;
using DMF.Utilities;
using Microsoft.Maui.Media;

namespace DMF.PageModels
{
    public partial class EditProfilePageModel : ObservableObject
    {
        private readonly IUserDetailService _userDetailService;
        private readonly IAuthService _authService;
        private readonly ISecureStorageService _storage;
        private readonly IBlobService _blobService;

        private int _userId;
        private int _dealerId;

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

        // Image source bound to the avatar: the uploaded photo URL, or the bundled
        // placeholder when the dealer has not set one yet.
        [ObservableProperty] private string profileImageDisplay = "profile";
        [ObservableProperty] private bool isUploadingPhoto;

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
            ISecureStorageService storage,
            IBlobService blobService)
        {
            _userDetailService = userDetailService;
            _authService = authService;
            _storage = storage;
            _blobService = blobService;
        }

        public async Task InitializeAsync()
        {
            var idStr = await _storage.GetAsync(AppConstants.UserId);
            int.TryParse(idStr, out _userId);

            var dealerStr = await _storage.GetAsync(AppConstants.DealersId);
            int.TryParse(dealerStr, out _dealerId);

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
            RefreshProfileImageDisplay();
        }

        // Shows the uploaded photo when ProfileImage is a real (http) URL; otherwise
        // falls back to the bundled placeholder avatar.
        private void RefreshProfileImageDisplay()
        {
            ProfileImageDisplay =
                _profileImage?.StartsWith("http", StringComparison.OrdinalIgnoreCase) == true
                    ? _profileImage
                    : "profile";
        }

        // Tapped from the camera badge: pick a photo (the OS picker shows its own
        // crop UI where available), square it for a clean avatar, upload it to
        // profiles/{dealerId}/ in blob storage, and preview it immediately.
        [RelayCommand]
        private async Task ChangePhoto()
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select profile photo"
                });

                if (photo == null) return; // cancelled

                IsUploadingPhoto = true;

                var ownerId = _dealerId > 0 ? _dealerId : _userId;
                var blobPath = $"profiles/{ownerId}/{Guid.NewGuid():N}.jpg";

                using var stream = ImageHelper.CropToSquare(photo.FullPath, 512);
                var url = await _blobService.UploadAsync(stream, blobPath, "image/jpeg");

                _profileImage = url;
                RefreshProfileImageDisplay();
            }
            catch (Exception ex)
            {
                await Shell.Current.CurrentPage.DisplayAlert(
                    "Photo", "Could not update the photo: " + ex.Message, "OK");
            }
            finally
            {
                IsUploadingPhoto = false;
            }
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

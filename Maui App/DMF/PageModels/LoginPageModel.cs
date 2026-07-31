using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.DTOs.User;
using System.Text;

namespace DMF.PageModels
{
    public partial class LoginPageModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanGetOtp))]
        [NotifyPropertyChangedFor(nameof(GetOtpButtonColor))]
        private bool isTermsAccepted;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanGetOtp))]
        [NotifyPropertyChangedFor(nameof(GetOtpButtonColor))]
        private string mobileNumber = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanGetOtp))]
        [NotifyPropertyChangedFor(nameof(GetOtpButtonColor))]
        private string name = string.Empty;

        // GET OTP is "ready" once the name, a valid 10-digit mobile, and the terms
        // checkbox are all in place — the button highlights in the app red then.
        public bool CanGetOtp =>
            !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(MobileNumber)
            && System.Text.RegularExpressions.Regex.IsMatch(MobileNumber, @"^\d{10}$")
            && IsTermsAccepted;

        public Color GetOtpButtonColor => CanGetOtp
            ? Color.FromArgb("#CA2F49")   // DmfRed — consistent app accent
            : Color.FromArgb("#929292");  // DmfGray — inactive

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool hasMobileError = false;

        [ObservableProperty]
        private bool hasNameError = false;

        public StringBuilder message;

        private readonly IAuthService _authService;
        private readonly IPopupService _popupService;
        private readonly IUserDetailService _userDetailService;

        public LoginPageModel(IAuthService authService, IUserDetailService userDetailService, IPopupService popupService)
        {
            IsBusy = false;
            isTermsAccepted = false;
            _authService = authService;
            _popupService = popupService;
            _userDetailService = userDetailService;
            message = new StringBuilder();
        }

        public async void NavigateToHomePage()
        {
            await Shell.Current.GoToAsync("///HomeTab/HomePage");
        }

        [RelayCommand]
        private async Task NavigateToOTPVerification()
        {
            if (verifyCanSubmit())
            {

                await _popupService.ShowPopupAsync(new PopupModel { PopupName = "Validation Error", PopupMessage = message.ToString() });
                return;
            }

            IsBusy = true;
            try
            {
                var response = await _userDetailService.CreateAsync(new UserDetailDto()
                {
                    PrimaryMobile = MobileNumber,
                    FirstName = Name,
                    IsDealers = false,
                    ProfileImage = "default.png",
                    IsActive = true
                });

                if (!response.Success || response.Data.UserDetail == null)
                {
                    await _popupService.ShowPopupAsync(new PopupModel { PopupName = "Error", PopupMessage = response.Message ?? "Something went wrong." });
                    return;
                }

                // OTP was already sent by the server during CreateAsync — fetch the hint
                var otpResult = await _authService.SendOtpAsync(MobileNumber);

                var navigationParameter = new Dictionary<string, object>
                {
                    { "MobileNumber", MobileNumber },
                    { "UserDetail", response.Data.UserDetail },
                    { "OtpHint", otpResult.Data ?? string.Empty }
                };
                await Shell.Current.GoToAsync($"otpverification", navigationParameter);
            }
            catch (Exception)
            {
                await _popupService.ShowPopupAsync(new PopupModel { PopupName = "Connection Error", PopupMessage = "• Could not reach the server. Please check your connection." });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool verifyCanSubmit()
        {
            message = new StringBuilder();

            if (string.IsNullOrWhiteSpace(Name))
                message.AppendLine("• Please enter your name.");

            if (string.IsNullOrWhiteSpace(MobileNumber))
                message.AppendLine("• Please enter your mobile number.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(MobileNumber, @"^\d{10}$"))
                message.AppendLine("• Mobile number must be 10 digits.");

            return message.Length > 0;
        }
    }
}

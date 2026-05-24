using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.DTOs.Auth;
using DMF.DTOs.User;
using System.Timers;

namespace DMF.PageModels
{
    public partial class OTPVerificationPageModel : ObservableObject, IQueryAttributable
    {
        private System.Timers.Timer? _timer;
        private const int InitialSeconds = 59;
        private readonly IAuthService _authService;
        private readonly IPopupService _popupService;
        private readonly ISecureStorageService _storageService;

        [ObservableProperty]
        private int remainingSeconds = InitialSeconds;

        [ObservableProperty]
        private string mobileNumber = string.Empty;

        [ObservableProperty]
        private UserDetailDto userDetail = new();

        [ObservableProperty]
        private bool isResendEnabled;

        [ObservableProperty]
        private bool isBusy = false;

        private readonly IUserDetailService _userDetailService;

        public OTPVerificationPageModel(IAuthService authService, IPopupService popupService, ISecureStorageService storageService, IUserDetailService userDetailService)
        {
            otp1 = string.Empty;
            otp2 = string.Empty;
            otp3 = string.Empty;
            otp4 = string.Empty;
            IsBusy = false;
            StartTimer();

            _authService = authService;
            _popupService = popupService;
            _storageService = storageService;
            _userDetailService = userDetailService;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("MobileNumber") && query["MobileNumber"] is not null)
            {
                MobileNumber = query["MobileNumber"]?.ToString() ?? string.Empty;
            }
            if (query.ContainsKey("UserDetail") && query["UserDetail"] is not null)
            {
                UserDetail = (UserDetailDto)query["UserDetail"];
            }
        }

        /* ================= TIMER ================= */

        public string RemainingTimeText =>
            RemainingSeconds > 0
                ? $"Remaining {RemainingSeconds} sec."
                : "You can resend OTP now";

        private void StartTimer()
        {
            IsResendEnabled = false;
            RemainingSeconds = InitialSeconds;

            _timer?.Stop();
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (RemainingSeconds > 0)
            {
                RemainingSeconds--;
                OnPropertyChanged(nameof(RemainingTimeText));
            }
            else
            {
                _timer.Stop();
                IsResendEnabled = true;
            }
        }

        /* ================= OTP INPUT ================= */

        [ObservableProperty] private string otp1;
        [ObservableProperty] private string otp2;
        [ObservableProperty] private string otp3;
        [ObservableProperty] private string otp4;

        public bool IsVerifyEnabled =>
            !string.IsNullOrWhiteSpace(Otp1) &&
            !string.IsNullOrWhiteSpace(Otp2) &&
            !string.IsNullOrWhiteSpace(Otp3) &&
            !string.IsNullOrWhiteSpace(Otp4);

        partial void OnOtp1Changed(string value) => NotifyVerifyState();
        partial void OnOtp2Changed(string value) => NotifyVerifyState();
        partial void OnOtp3Changed(string value) => NotifyVerifyState();
        partial void OnOtp4Changed(string value) => NotifyVerifyState();

        private void NotifyVerifyState()
        {
            OnPropertyChanged(nameof(IsVerifyEnabled));
        }

        /* ================= COMMANDS ================= */

        [RelayCommand]
        private async Task VerifyOtp()
        {
            IsBusy = true;

            var otp = $"{Otp1}{Otp2}{Otp3}{Otp4}";

            var response = await _authService.VerifyOtpAsync(new VerifyOtpRequestDto
            {
                Mobile = MobileNumber,
                Otp = otp
            });
            if (!response.Success)
            {
                IsBusy = false;
                await _popupService.ShowPopupAsync(new PopupModel { PopupName = "Validation Error", PopupMessage = "• OTP Verification Failed." },
                    onOk: () =>
                    {
                        Otp1 = string.Empty;
                        Otp2 = string.Empty;
                        Otp3 = string.Empty;
                        Otp4 = string.Empty;
                    });
                return;
            }
            // Fetch real user to get correct ID
            var userResponse = await _userDetailService.GetByMobileNoAsync(MobileNumber);
            if (userResponse.Success && userResponse.Data != null)
                UserDetail = userResponse.Data;

            SetSecureStorage(response.Data);
            IsBusy = false;
            await Shell.Current.GoToAsync("///mainPage");
        }

        [RelayCommand(CanExecute = nameof(IsResendEnabled))]
        private async Task ResendOtp()
        {
            if (RemainingSeconds == 0)
                StartTimer();
            await _authService.SendOtpAsync(MobileNumber);
        }

        [RelayCommand]
        private async Task EditMobileNumber()
        {
            await Shell.Current.GoToAsync("///login");
        }

        private void SetSecureStorage(AuthResponseDto? auth)
        {
            if (auth != null)
            {
                _storageService.SetAsync(AppConstants.UserName, UserDetail.FirstName);
                _storageService.SetAsync(AppConstants.UserId, UserDetail.ID.ToString());
                _storageService.SetAsync(AppConstants.OauthToken, AppConstants.OauthToken);
                _storageService.SetAsync(AppConstants.UserMobile, UserDetail.PrimaryMobile);
                _storageService.SetAsync(AppConstants.IsDealers, UserDetail.IsDealers.ToString());

                if (!string.IsNullOrEmpty(UserDetail.Email))
                    _storageService.SetAsync(AppConstants.UserEmail, UserDetail.Email);
            }
        }
    }
}

using DMF.Constants;
using DMF.DTOs.Auth;

namespace DMF.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;

        public AuthService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public bool IsAuthenticated =>
        !string.IsNullOrEmpty(SecureStorage.GetAsync(AppKeys.AuthToken).Result);

        // ---------- SEND OTP ----------
        public async Task<ApiResponse<string>> SendOtpAsync(string mobile)
        {
            return await _apiService.PostAsync<SendOtpRequestDto, string>(
                "auth/send-otp",
                new SendOtpRequestDto { Mobile = mobile });
        }

        // ---------- VERIFY OTP ----------
        public async Task<ApiResponse<AuthResponseDto>> VerifyOtpAsync(
            VerifyOtpRequestDto request)
        {
            var response = await _apiService
                .PostAsync<VerifyOtpRequestDto, AuthResponseDto>("auth/verify-otp", request);

            if (response.Success && response.Data != null)
            {
                await SecureStorage.SetAsync(AppKeys.AuthToken, response.Data.Token);
            }

            return response;
        }

        // ---------- LOGIN WITH EMAIL ----------
        public async Task<ApiResponse<AuthResponseDto>> LoginWithEmailAsync(EmailLoginRequestDto request)
        {
            var response = await _apiService
                .PostAsync<EmailLoginRequestDto, AuthResponseDto>("auth/login", request);

            if (response.Success && response.Data != null)
            {
                await SecureStorage.SetAsync(AppKeys.AuthToken, response.Data.Token);
            }

            return response;
        }

        // ---------- SET PASSWORD ----------
        public async Task<ApiResponse<bool>> SetPasswordAsync(SetPasswordRequestDto request)
        {
            return await _apiService
                .PostAsync<SetPasswordRequestDto, bool>("auth/set-password", request);
        }

        // ---------- GET TOKEN ----------
        public async Task<string?> GetTokenAsync()
        {
            return await SecureStorage.GetAsync(AppKeys.AuthToken);
        }

        // ---------- LOGOUT ----------
        public async Task LogoutAsync()
        {
            // Clear auth tokens
            SecureStorage.Remove(AppKeys.AuthToken);
            SecureStorage.Remove(AppKeys.RefreshToken);
            SecureStorage.Remove(AppKeys.TokenExpiry);

            // Clear all user data so a new login always starts fresh
            SecureStorage.Remove(AppConstants.UserId);
            SecureStorage.Remove(AppConstants.DealersId);
            SecureStorage.Remove(AppConstants.UserName);
            SecureStorage.Remove(AppConstants.UserMobile);
            SecureStorage.Remove(AppConstants.UserEmail);
            SecureStorage.Remove(AppConstants.UserCity);
            SecureStorage.Remove(AppConstants.IsDealers);

            await Task.CompletedTask;
        }
    }
}

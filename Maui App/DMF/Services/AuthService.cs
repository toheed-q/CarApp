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
        public async Task<ApiResponse<bool>> SendOtpAsync(string mobile)
        {
            return await _apiService.PostAsync<SendOtpRequestDto, bool>(
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

        // ---------- GET TOKEN ----------
        public async Task<string?> GetTokenAsync()
        {
            return await SecureStorage.GetAsync(AppKeys.AuthToken);
        }

        // ---------- LOGOUT ----------
        public async Task LogoutAsync()
        {
            SecureStorage.Remove(AppKeys.AuthToken);
            SecureStorage.Remove(AppKeys.RefreshToken);
            SecureStorage.Remove(AppKeys.TokenExpiry);
            await Task.CompletedTask;
        }
    }
}

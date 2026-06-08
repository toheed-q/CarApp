using DMF.DTOs.Auth;

namespace DMF.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<string>> SendOtpAsync(string mobile);
        Task<ApiResponse<AuthResponseDto>> VerifyOtpAsync(VerifyOtpRequestDto request);
        Task<ApiResponse<AuthResponseDto>> LoginWithEmailAsync(EmailLoginRequestDto request);
        Task<ApiResponse<bool>> SetPasswordAsync(SetPasswordRequestDto request);
        Task<string?> GetTokenAsync();
        Task LogoutAsync();
        bool IsAuthenticated { get; }
    }
}
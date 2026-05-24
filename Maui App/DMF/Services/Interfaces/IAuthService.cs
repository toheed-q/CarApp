using DMF.DTOs.Auth;

namespace DMF.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<bool>> SendOtpAsync(string mobile);

        Task<ApiResponse<AuthResponseDto>> VerifyOtpAsync(VerifyOtpRequestDto request);

        Task<string?> GetTokenAsync();

        Task LogoutAsync();

        bool IsAuthenticated { get; }
    }
}
using DMF_Services.DTOs.Auth;
using DMF_Services.Helpers;

namespace DMF_Services.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<string>> SendOtpAsync(string mobile);
        Task<ApiResponse<AuthResponseDto>> VerifyOtpAsync(VerifyOtpRequestDto dto);
        Task<ApiResponse<AuthResponseDto>> LoginWithEmailAsync(EmailLoginRequestDto dto);
        Task<ApiResponse<bool>> SetPasswordAsync(SetPasswordRequestDto dto);
    }
}

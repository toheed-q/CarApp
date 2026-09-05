using Asp.Versioning;
using DMF_Services.DTOs.Auth;
using DMF_Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DMF_Services.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp(SendOtpRequestDto dto)
            => Ok(await _auth.SendOtpAsync(dto.Mobile));

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp(SendOtpRequestDto dto)
            => Ok(await _auth.ResendOtpAsync(dto.Mobile));

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequestDto dto)
            => Ok(await _auth.VerifyOtpAsync(dto));

        [HttpPost("login")]
        public async Task<IActionResult> LoginWithEmail(EmailLoginRequestDto dto)
            => Ok(await _auth.LoginWithEmailAsync(dto));

        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword(SetPasswordRequestDto dto)
            => Ok(await _auth.SetPasswordAsync(dto));
    }
}

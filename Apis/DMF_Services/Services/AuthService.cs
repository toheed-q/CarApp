using DMF_Services.Data;
using DMF_Services.DTOs.Auth;
using DMF_Services.Helpers;
using DMF_Services.Models;
using DMF_Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMF_Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            AppDbContext dbContext,
            IJwtTokenService jwtTokenService)
        {
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
        }

        // -------------------- SEND OTP --------------------
        public async Task<ApiResponse<string>> SendOtpAsync(string mobile)
        {
            var otp = Random.Shared.Next(1000, 9999).ToString();
            var now = DateTime.Now;

            // Check for existing unused OTP
            var existingOtp = await _dbContext.UserOtps
                .FirstOrDefaultAsync(x => x.Mobile == mobile && !x.IsUsed);

            if (existingOtp != null)
            {
                // UPDATE existing OTP
                existingOtp.OtpCode = otp;
                existingOtp.ExpiryTime = now.AddMinutes(5);
                existingOtp.CreatedOn = now;

                _dbContext.UserOtps.Update(existingOtp);
            }
            else
            {
                // CREATE new OTP
                var userOtp = new UserOtp
                {
                    Mobile = mobile,
                    OtpCode = otp,
                    ExpiryTime = now.AddMinutes(5),
                    IsUsed = false,
                    CreatedOn = now
                };

                _dbContext.UserOtps.Add(userOtp);
            }

            await _dbContext.SaveChangesAsync();

            // TODO: Integrate SMS gateway here

            return new ApiResponse<string>
            {
                Success = true,
                Message = "OTP sent successfully",
                Data = otp
            };
        }

        // -------------------- VERIFY OTP --------------------
        public async Task<ApiResponse<AuthResponseDto>> VerifyOtpAsync(
            VerifyOtpRequestDto dto)
        {
            var otpRecord = await _dbContext.UserOtps
                .Where(x => x.Mobile == dto.Mobile
                         && x.OtpCode == dto.Otp
                         && !x.IsUsed)
                .OrderByDescending(x => x.CreatedOn)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "Invalid OTP"
                };
            }

            if (otpRecord.ExpiryTime < DateTime.Now)
            {
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "OTP expired"
                };
            }

            otpRecord.IsUsed = true;

            var user = await _dbContext.UserDetails
                .FirstOrDefaultAsync(x => x.PrimaryMobile == dto.Mobile);

            bool isNewUser = false;

            if (user == null)
            {
                isNewUser = true;

                user = new UserDetail
                {
                    PrimaryMobile = dto.Mobile,
                    FirstName = "Guest",
                    ProfileImage = "default.png",
                    IsDealers = false
                };

                _dbContext.UserDetails.Add(user);
                await _dbContext.SaveChangesAsync();
            }

            var token = _jwtTokenService.GenerateToken(user.ID, dto.Mobile);

            await _dbContext.SaveChangesAsync();

            return new ApiResponse<AuthResponseDto>
            {
                Success = true,
                Message = "OTP verified successfully",
                Data = new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = DateTime.Now.AddDays(7),
                    IsNewUser = isNewUser
                }
            };
        }
    }
}

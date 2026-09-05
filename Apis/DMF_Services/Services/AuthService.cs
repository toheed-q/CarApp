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
        private readonly IOtpService _otpService;

        public AuthService(
            AppDbContext dbContext,
            IJwtTokenService jwtTokenService,
            IOtpService otpService)
        {
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
            _otpService = otpService;
        }

        // Local/dev fallback (when Fast2SMS is disabled): a single fixed OTP is
        // accepted so the flow can be tested without spending SMS credits. In
        // production Fast2SMS is enabled and this bypass is NOT accepted.
        private const string ClosedTestingOtp = "4455";

        // -------------------- SEND OTP --------------------
        public async Task<ApiResponse<string>> SendOtpAsync(string mobile)
        {
            // Production: Fast2SMS generates and sends the OTP. We store nothing
            // and never learn the code — verification is delegated to Fast2SMS.
            if (_otpService.Enabled)
            {
                var result = await _otpService.SendAsync(mobile);
                return new ApiResponse<string>
                {
                    Success = result.Success,
                    Message = result.Message ?? (result.Success ? "OTP sent successfully" : "Failed to send OTP"),
                    Data = null
                };
            }

            // Dev fallback: issue the fixed closed-testing OTP via the DB.
            await UpsertClosedTestingOtpAsync(mobile);
            return new ApiResponse<string>
            {
                Success = true,
                Message = "OTP sent successfully",
                Data = ClosedTestingOtp
            };
        }

        // -------------------- RESEND OTP --------------------
        public async Task<ApiResponse<string>> ResendOtpAsync(string mobile)
        {
            if (_otpService.Enabled)
            {
                var result = await _otpService.ResendAsync(mobile);
                return new ApiResponse<string>
                {
                    Success = result.Success,
                    Message = result.Message ?? (result.Success ? "OTP resent successfully" : "Failed to resend OTP"),
                    Data = null
                };
            }

            await UpsertClosedTestingOtpAsync(mobile);
            return new ApiResponse<string>
            {
                Success = true,
                Message = "OTP resent successfully",
                Data = ClosedTestingOtp
            };
        }

        // Creates/refreshes the fixed dev OTP row (dev fallback only).
        private async Task UpsertClosedTestingOtpAsync(string mobile)
        {
            var now = DateTime.Now;
            var existingOtp = await _dbContext.UserOtps
                .FirstOrDefaultAsync(x => x.Mobile == mobile && !x.IsUsed);

            if (existingOtp != null)
            {
                existingOtp.OtpCode = ClosedTestingOtp;
                existingOtp.ExpiryTime = now.AddMinutes(5);
                existingOtp.CreatedOn = now;
                _dbContext.UserOtps.Update(existingOtp);
            }
            else
            {
                _dbContext.UserOtps.Add(new UserOtp
                {
                    Mobile = mobile,
                    OtpCode = ClosedTestingOtp,
                    ExpiryTime = now.AddMinutes(5),
                    IsUsed = false,
                    CreatedOn = now
                });
            }

            await _dbContext.SaveChangesAsync();
        }

        // -------------------- VERIFY OTP --------------------
        public async Task<ApiResponse<AuthResponseDto>> VerifyOtpAsync(
            VerifyOtpRequestDto dto)
        {
            if (_otpService.Enabled)
            {
                // Production: Fast2SMS is the source of truth for the OTP.
                var verify = await _otpService.VerifyAsync(dto.Mobile, dto.Otp);
                if (!verify.Success)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = verify.Message ?? "Invalid OTP"
                    };
                }
            }
            else
            {
                // Dev fallback: validate against the fixed OTP / DB row.
                var isClosedTestingOtp = dto.Otp == ClosedTestingOtp;

                var otpRecord = await _dbContext.UserOtps
                    .Where(x => x.Mobile == dto.Mobile
                             && x.OtpCode == dto.Otp
                             && !x.IsUsed)
                    .OrderByDescending(x => x.CreatedOn)
                    .FirstOrDefaultAsync();

                if (otpRecord == null && !isClosedTestingOtp)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid OTP"
                    };
                }

                if (otpRecord != null)
                {
                    if (otpRecord.ExpiryTime < DateTime.Now && !isClosedTestingOtp)
                    {
                        return new ApiResponse<AuthResponseDto>
                        {
                            Success = false,
                            Message = "OTP expired"
                        };
                    }

                    otpRecord.IsUsed = true;
                }
            }

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
        // -------------------- LOGIN WITH EMAIL --------------------
        public async Task<ApiResponse<AuthResponseDto>> LoginWithEmailAsync(EmailLoginRequestDto dto)
        {
            var user = await _dbContext.UserDetails
                .FirstOrDefaultAsync(x => x.Email == dto.Email && x.IsActive == true);

            if (user == null)
            {
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "No account found with this email."
                };
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "Password not set. Please use OTP login or set a password from your profile."
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "Incorrect password."
                };
            }

            var token = _jwtTokenService.GenerateToken(user.ID, user.PrimaryMobile);

            return new ApiResponse<AuthResponseDto>
            {
                Success = true,
                Message = "Login successful",
                Data = new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = DateTime.Now.AddDays(7),
                    IsNewUser = false
                }
            };
        }

        // -------------------- SET PASSWORD --------------------
        public async Task<ApiResponse<bool>> SetPasswordAsync(SetPasswordRequestDto dto)
        {
            var user = await _dbContext.UserDetails.FindAsync(dto.UserId);

            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            await _dbContext.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Password set successfully.",
                Data = true
            };
        }
    }
}

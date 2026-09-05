namespace DMF_Services.Services.Interfaces
{
    // Abstraction over the SMS OTP provider (Fast2SMS). The provider itself
    // generates, sends and verifies the OTP — we only proxy the calls so the
    // API key never leaves the server.
    public interface IOtpService
    {
        // When false (local/dev), the caller should fall back to the fixed
        // closed-testing OTP instead of spending real SMS credits.
        bool Enabled { get; }

        Task<OtpResult> SendAsync(string mobile);
        Task<OtpResult> ResendAsync(string mobile);
        Task<OtpResult> VerifyAsync(string mobile, string otp);
    }

    public class OtpResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public static OtpResult Ok(string? message = null) => new() { Success = true, Message = message };
        public static OtpResult Fail(string? message) => new() { Success = false, Message = message };
    }
}

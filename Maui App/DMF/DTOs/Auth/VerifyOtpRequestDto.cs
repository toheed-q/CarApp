namespace DMF.DTOs.Auth
{
    public class VerifyOtpRequestDto
    {
        public string Mobile { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}

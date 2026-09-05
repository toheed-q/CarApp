namespace DMF_Services.Helpers
{
    // Bound from the "Fast2Sms" section of appsettings.json (git-ignored).
    public class Fast2SmsOptions
    {
        public bool Enabled { get; set; }
        public string BaseUrl { get; set; } = "https://www.fast2sms.com/dev/otp";
        public string ApiKey { get; set; } = string.Empty;
        public string OtpId { get; set; } = string.Empty;
        public int OtpExpiryMinutes { get; set; } = 5;
        public int OtpLength { get; set; } = 4;
    }
}

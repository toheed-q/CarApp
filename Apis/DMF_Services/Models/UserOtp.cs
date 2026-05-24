namespace DMF_Services.Models
{
    public class UserOtp
    {
        public int ID { get; set; }

        public string Mobile { get; set; } = string.Empty;

        public string OtpCode { get; set; } = string.Empty;

        public DateTime ExpiryTime { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}

namespace DMF_Services.Models
{
    // A user's request to be granted dealer access. An admin reviews these and
    // manually flips UserDetail.IsDealers = 1 for the matching account.
    public class DealerRequest
    {
        public int ID { get; set; }

        // The existing user account this request belongs to.
        public int UserDetailId { get; set; }

        // Submitted details (mirrors the profile form, no password).
        public string FullName { get; set; } = string.Empty;
        public string PrimaryMobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? CompanyName { get; set; }
        public string? Address1 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }

        // Workflow
        public string Status { get; set; } = "Pending";
        public DateTime CreatedDate { get; set; }
    }
}

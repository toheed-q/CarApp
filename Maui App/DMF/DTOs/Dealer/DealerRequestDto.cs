namespace DMF.DTOs.Dealer
{
    // Returned by the API after a dealer request is created.
    public class DealerRequestDto
    {
        public int ID { get; set; }
        public int UserDetailId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PrimaryMobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? CompanyName { get; set; }
        public string? Address1 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedDate { get; set; }
    }
}

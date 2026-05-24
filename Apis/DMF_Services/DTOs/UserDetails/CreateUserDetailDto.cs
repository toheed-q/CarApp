namespace DMF_Services.DTOs.UserDetails
{
    public class CreateUserDetailDto
    {
        // Personal Info
        public string FirstName { get; set; } = string.Empty;
        public string? MidleName { get; set; }
        public string? LastName { get; set; }

        // Company Info
        public string? CompanyName { get; set; }

        // Contact Info
        public string PrimaryMobile { get; set; } = string.Empty;
        public string? SecondaryMobile { get; set; }
        public string? Email { get; set; }

        // Address
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }

        // Profile
        public string ProfileImage { get; set; } = string.Empty;

        // Role
        public bool IsDealers { get; set; }

        // Services
        public bool? LoanService { get; set; }
        public bool? RegistrationService { get; set; }
        public bool? NocService { get; set; }

        public bool? IsActive { get; set; }
    }
}

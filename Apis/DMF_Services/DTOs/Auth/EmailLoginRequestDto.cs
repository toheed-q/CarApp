namespace DMF_Services.DTOs.Auth
{
    public class EmailLoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

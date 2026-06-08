namespace DMF.DTOs.Auth
{
    public class SetPasswordRequestDto
    {
        public int UserId { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}

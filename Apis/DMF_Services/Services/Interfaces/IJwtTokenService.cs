namespace DMF_Services.Services.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(int userId, string mobile);
    }
}

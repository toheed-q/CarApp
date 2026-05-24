using DMF_Services.DTOs.UserDetails;

namespace DMF_Services.Services.Interfaces
{
    public interface IUserDetailService
    {
        Task<IEnumerable<UserDetailDto>> GetAllAsync();
        Task<UserDetailDto?> GetByIdAsync(int id);
        Task<UserDetailDto?> GetByMobileNoAsync(string mobile, bool isActive = true);
        Task<(UserDetailDto UserDetail, bool IsCreated)> CreateAsync(CreateUserDetailDto dto);
        Task UpdateAsync(int id, UpdateUserDetailDto dto);
    }
}

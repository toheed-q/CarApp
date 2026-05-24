using DMF.DTOs.User;

namespace DMF.Services.Interfaces
{
    public interface IUserDetailService
    {
        Task<ApiResponse<IEnumerable<UserDetailDto>>> GetAllAsync();

        Task<ApiResponse<UserDetailDto?>> GetByIdAsync(int id);

        Task<ApiResponse<UserDetailDto?>> GetByMobileNoAsync(string mobile, bool isActive = true);

        Task<ApiResponse<(UserDetailDto UserDetail, bool IsCreated)>> CreateAsync(UserDetailDto dto);

        Task<ApiResponse<bool>> UpdateAsync(int id, UserDetailDto dto);
    }
}

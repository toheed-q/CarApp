using DMF_Services.DTOs.Cars;
using DMF_Services.Helpers;

namespace DMF_Services.Services.Interfaces
{
    public interface ICarWishlistService
    {
        Task<ApiResponse<IEnumerable<CarFilterResultDto>>> GetWishlistCarsAsync(int userDetailId);
        Task<ApiResponse<bool>> ToggleWishlistAsync(int userDetailId, int carDetailId);
    }
}

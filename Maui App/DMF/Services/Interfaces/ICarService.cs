namespace DMF.Services.Interfaces
{
    public interface ICarService
    {
        Task<List<CarModel>> GetCarsAsync();
        Task<ApiResponse<IEnumerable<CarFilterResult>>> GetFavoriteCarsAsync(int userId);

        Task<ApiResponse<IEnumerable<CarModel>>> GetAllCarsAsync();

        Task<ApiResponse<CarModel>> GetCarByIdAsync(int id);

        Task<ApiResponse<PagedResponse<CarFilterResult>>> GetFilteredCarsAsync(CarFilterModel f);

        Task<ApiResponse<bool>> ToggleWishlistAsync(int userId, int carId);

        Task<List<string>?> GetBrandsAsync();
        Task<List<string>?> GetModelsAsync();
        Task<ApiResponse<bool>> AddCarAsync(AddCarModel model, IEnumerable<ImageItem> images, string dealerName, int dealerId, Func<double, Task>? progressCallback = null);
    }
}

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
        Task<ApiResponse<bool>> UpdateCarAsync(AddCarModel model, IEnumerable<ImageItem> images, string dealerName, int dealerId, Func<double, Task>? progressCallback = null);
        Task<ApiResponse<PagedResponse<CarFilterResult>>> GetDealerCarsAsync(int dealersId, int page = 1, int pageSize = 20);

        // Fetch a single car by id in the full list shape (images/location/distance),
        // used to open a car from a deep-link / share URL. Returns null if not found.
        Task<CarFilterResult?> GetCarForShareAsync(int carId);
        Task<ApiResponse<bool>> DeleteCarAsync(int carId);
    }
}

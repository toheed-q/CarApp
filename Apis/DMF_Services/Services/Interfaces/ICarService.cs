using DMF_Services.DTOs.Cars;
using DMF_Services.DTOs.Common;

namespace DMF_Services.Services.Interfaces
{
    public interface ICarService
    {
        Task<IEnumerable<CarDetailDto>> GetAllAsync();
        Task<CarDetailDto?> GetByIdAsync(int id);
        Task<PagedResponse<CarFilterResultDto>> GetFilteredCarsAsync(string? brand, string? model, string? search, string? fuel, string? transmission, int owners, int priceMore, int priceLess, int drivenMore, int drivenLess, int age, int userDetailID, int dealersID, int isActive, int page, int pageSize, string sortBy, string sortDir, double? buyerLat = null, double? buyerLon = null, int? cityId = null);
        Task<int> CreateCarAsync(CreateCarDto dto);
        Task UpdateCarImagesAsync(int carId, List<string> imageUrls);
        Task<bool> DeleteCarAsync(int carId);
    }
}

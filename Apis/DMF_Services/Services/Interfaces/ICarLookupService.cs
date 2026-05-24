using DMF_Services.DTOs.Cars;

namespace DMF_Services.Services.Interfaces
{
    public interface ICarLookupService
    {
        Task<IEnumerable<CarBrandDto>> GetBrandsAsync();
        Task<IEnumerable<CarModelDto>> GetModelsByBrandAsync(string brand);
        Task<IEnumerable<CarBrandWithModelsDto>> GetBrandsWithModelsAsync();
    }
}

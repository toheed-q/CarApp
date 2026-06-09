using DMF.DTOs.Cities;

namespace DMF.Services
{
    public class CityService : ICityService
    {
        private readonly IApiService _apiService;

        public CityService(IApiService apiService)
        {
            _apiService = apiService;
        }

        // --------------------------------------------------
        // GET ACTIVE CITIES  ->  GET /api/1.0/cities
        // Pure API pass-through; keeps the ApiResponse envelope intact.
        // --------------------------------------------------
        public async Task<ApiResponse<List<CityDto>>> GetActiveCitiesAsync()
        {
            return await _apiService.GetAsync<List<CityDto>>("cities");
        }
    }
}

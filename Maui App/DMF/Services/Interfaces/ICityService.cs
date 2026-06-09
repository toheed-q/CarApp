using DMF.DTOs.Cities;

namespace DMF.Services.Interfaces
{
    public interface ICityService
    {
        /// <summary>
        /// Fetches the active cities from the backend (GET /api/1.0/cities).
        /// Preserves the ApiResponse envelope so callers can inspect Success/Message.
        /// </summary>
        Task<ApiResponse<List<CityDto>>> GetActiveCitiesAsync();
    }
}

using DMF_Services.DTOs.Cities;

namespace DMF_Services.Services.Interfaces
{
    public interface ICityService
    {
        /// <summary>
        /// Returns all active cities (IsActive = true), ordered by CityName ASC.
        /// Read-only; never throws — on failure an empty list is returned and logged.
        /// </summary>
        Task<List<CityDto>> GetActiveCitiesAsync();
    }
}

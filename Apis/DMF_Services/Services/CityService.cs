using DMF_Services.Data;
using DMF_Services.DTOs.Cities;
using DMF_Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMF_Services.Services
{
    public class CityService : ICityService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<CityService> _logger;

        public CityService(AppDbContext db, ILogger<CityService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // -----------------------------
        // Get active cities (read-only)
        // -----------------------------
        public async Task<List<CityDto>> GetActiveCitiesAsync()
        {
            try
            {
                // Project straight to CityDto so Latitude/Longitude never leave the DB layer
                // and only the two needed columns are read.
                var cities = await _db.CityLocations
                    .AsNoTracking()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CityName)
                    .Select(c => new CityDto
                    {
                        Id       = c.Id,
                        CityName = c.CityName
                    })
                    .ToListAsync();

                _logger.LogInformation("Active cities fetched. Count={Count}.", cities.Count);

                return cities;
            }
            catch (Exception ex)
            {
                // Read-only lookup must never break the caller. Log and degrade to empty list.
                _logger.LogError(ex, "Failed to fetch active cities. Returning empty list.");
                return new List<CityDto>();
            }
        }
    }
}

using Asp.Versioning;
using DMF_Services.DTOs.Cities;
using DMF_Services.Helpers;
using DMF_Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DMF_Services.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityService _service;
        private readonly ILogger<CitiesController> _logger;

        public CitiesController(ICityService service, ILogger<CitiesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ----------------------------
        // GET: api/1.0/cities
        // Returns active cities only.
        // ----------------------------
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CityDto>>>> GetActiveCities()
        {
            _logger.LogInformation("Active cities requested.");

            // Service is exception-safe and returns [] on failure, so this stays 200 OK.
            var cities = await _service.GetActiveCitiesAsync();

            _logger.LogInformation("Active cities response prepared. Count={Count}.", cities.Count);

            return Ok(new ApiResponse<List<CityDto>>
            {
                Success = true,
                Message = "Active cities fetched successfully",
                Data = cities
            });
        }
    }
}

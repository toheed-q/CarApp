using Asp.Versioning;
using DMF_Services.DTOs.Cars;
using DMF_Services.Helpers;
using DMF_Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DMF_Services.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/car-lookup")]
    public class CarLookupController : ControllerBase
    {
        private readonly ICarLookupService _service;

        public CarLookupController(ICarLookupService service)
        {
            _service = service;
        }

        // ----------------------------
        // GET: api/1.0/car-lookup/brands
        // ----------------------------
        [HttpGet("brands")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CarBrandDto>>>> GetBrands()
        {
            var brands = await _service.GetBrandsAsync();

            return Ok(new ApiResponse<IEnumerable<CarBrandDto>>
            {
                Success = true,
                Message = "Car brands fetched successfully",
                Data = brands
            });
        }

        // ----------------------------
        // GET: api/1.0/car-lookup/models?brand=Honda
        // ----------------------------
        [HttpGet("models")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CarModelDto>>>> GetModels(
            [FromQuery] string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
            {
                return BadRequest(new ApiResponse<IEnumerable<CarModelDto>>
                {
                    Success = false,
                    Message = "Brand is required"
                });
            }

            var models = await _service.GetModelsByBrandAsync(brand);

            return Ok(new ApiResponse<IEnumerable<CarModelDto>>
            {
                Success = true,
                Message = $"Models fetched for brand '{brand}'",
                Data = models
            });
        }

        // ----------------------------
        // GET: api/1.0/car-lookup/brands-models
        // ----------------------------
        [HttpGet("brands-models")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CarBrandWithModelsDto>>>> GetBrandsWithModels()
        {
            var data = await _service.GetBrandsWithModelsAsync();

            return Ok(new ApiResponse<IEnumerable<CarBrandWithModelsDto>>
            {
                Success = true,
                Message = "Brands with models fetched successfully",
                Data = data
            });
        }
    }
}

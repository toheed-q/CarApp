using Asp.Versioning;
using DMF_Services.DTOs.Cars;
using DMF_Services.DTOs.Common;
using DMF_Services.Helpers;
using DMF_Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DMF_Services.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/cars")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _service;

        public CarsController(ICarService service)
        {
            _service = service;
        }

        // ----------------------------------------------------
        // GET: api/1.0/cars
        // ----------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CarDetailDto>>>> GetAll()
        {
            var cars = await _service.GetAllAsync();

            return Ok(new ApiResponse<IEnumerable<CarDetailDto>>
            {
                Success = true,
                Message = "Cars fetched successfully",
                Data = cars
            });
        }

        // ----------------------------------------------------
        // GET: api/1.0/cars/{id}
        // ----------------------------------------------------
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<CarDetailDto>>> GetById(int id)
        {
            var car = await _service.GetByIdAsync(id);

            if (car == null)
            {
                return NotFound(new ApiResponse<CarDetailDto>
                {
                    Success = false,
                    Message = "Car not found"
                });
            }

            return Ok(new ApiResponse<CarDetailDto>
            {
                Success = true,
                Message = "Car fetched successfully",
                Data = car
            });
        }

        // ----------------------------------------------------
        // GET: api/1.0/cars/filter
        // ----------------------------------------------------
        // Example:
        // api/cars/filter?brand=Honda&fuel=Petrol&priceLess=800000&age=5
        // ----------------------------------------------------
        [HttpGet("filter")]
        public async Task<ActionResult<ApiResponse<PagedResponse<CarFilterResultDto>>>> Filter(
            [FromQuery] string? brand,
            [FromQuery] string? model,
            [FromQuery] string? search,
            [FromQuery] string? fuel,
            [FromQuery] string? transmission,
            [FromQuery] int owners = 0,
            [FromQuery] int priceMore = 0,
            [FromQuery] int priceLess = 0,
            [FromQuery] int drivenMore = 0,
            [FromQuery] int drivenLess = 0,
            [FromQuery] int age = 0,
            [FromQuery] int userDetailID = 0,
            [FromQuery] int dealersID = 0,
            [FromQuery] int isActive = 1,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "price",
            [FromQuery] string sortDir = "asc")
        {
            var cars = await _service.GetFilteredCarsAsync(
                brand,
                model,
                search,
                fuel,
                transmission,
                owners,
                priceMore,
                priceLess,
                drivenMore,
                drivenLess,
                age,
                userDetailID,
                dealersID,
                isActive,
                page,
                pageSize,
                sortBy,
                sortDir
            );

            return Ok(new ApiResponse<PagedResponse<CarFilterResultDto>>
            {
                Success = true,
                Message = "Filtered cars fetched successfully",
                Data = cars
            });
        }
    }
}

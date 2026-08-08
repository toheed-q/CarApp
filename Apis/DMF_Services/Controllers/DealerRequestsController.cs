using Asp.Versioning;
using DMF_Services.DTOs.DealerRequests;
using DMF_Services.Helpers;
using DMF_Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DMF_Services.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/dealer-requests")]
    public class DealerRequestsController : ControllerBase
    {
        private readonly IDealerRequestService _service;

        public DealerRequestsController(IDealerRequestService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DealerRequestDto>>> Create(CreateDealerRequestDto dto)
        {
            var data = await _service.CreateAsync(dto);

            return Ok(new ApiResponse<DealerRequestDto>
            {
                Success = true,
                Message = "Dealer request submitted successfully",
                Data = data
            });
        }
    }
}

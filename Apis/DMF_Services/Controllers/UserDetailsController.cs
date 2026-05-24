using Asp.Versioning;
using DMF_Services.DTOs.UserDetails;
using DMF_Services.Helpers;
using DMF_Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DMF_Services.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/user-details")]
    public class UserDetailsController : ControllerBase
    {
        private readonly IUserDetailService _service;
        private readonly IAuthService _authService;

        public UserDetailsController(IUserDetailService service, IAuthService authService)
        {
            _service = service;
            _authService = authService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserDetailDto>>>> GetAll()
        {
            var data = await _service.GetAllAsync();

            return Ok(new ApiResponse<IEnumerable<UserDetailDto>>
            {
                Success = true,
                Message = "User details fetched successfully",
                Data = data
            });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<UserDetailDto>>> Get(int id)
        {
            var data = await _service.GetByIdAsync(id);

            if (data == null)
            {
                return NotFound(new ApiResponse<UserDetailDto>
                {
                    Success = false,
                    Message = "User detail not found"
                });
            }

            return Ok(new ApiResponse<UserDetailDto>
            {
                Success = true,
                Message = "User detail fetched successfully",
                Data = data
            });
        }

        [HttpGet("get-user-by-mobile-no")]
        public async Task<ActionResult<ApiResponse<UserDetailDto>>> GetByMobileNo([FromQuery] string mobile, [FromQuery] bool isActive = true)
        {
            var data = await _service.GetByMobileNoAsync(mobile, isActive);

            if (data == null)
            {
                return NotFound(new ApiResponse<UserDetailDto>
                {
                    Success = false,
                    Message = "User detail not found"
                });
            }

            return Ok(new ApiResponse<UserDetailDto>
            {
                Success = true,
                Message = "User detail fetched successfully",
                Data = data
            });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDetailDto>>> Create(CreateUserDetailDto dto)
        {
            var (user, isCreated) = await _service.CreateAsync(dto);

            if (user != null)
            {
                await _authService.SendOtpAsync(user.PrimaryMobile);

                if (!isCreated)
                {
                    return Ok(new ApiResponse<UserDetailDto>
                    {
                        Success = true,
                        Message = "User already exists",
                        Data = user
                    });
                }

                return CreatedAtAction(nameof(Get), new { id = user }, new ApiResponse<UserDetailDto>
                {
                    Success = true,
                    Message = "User detail created successfully",
                    Data = user
                });
            }
            else
            {
                return BadRequest(new ApiResponse<UserDetailDto>
                {
                    Success = false,
                    Message = "Server error",
                    Data = user
                });
            }
        }
    }
}

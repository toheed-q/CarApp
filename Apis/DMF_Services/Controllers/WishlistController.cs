using Asp.Versioning;
using DMF_Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DMF_Services.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/Wishlist")]
    public class WishlistController : ControllerBase
    {
        private readonly ICarWishlistService _wishlistService;

        public WishlistController(ICarWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        // ⭐ Get Wishlist Cars by User
        [HttpGet("{userDetailId}")]
        public async Task<IActionResult> GetWishlistCars(int userDetailId)
        {
            var response = await _wishlistService.GetWishlistCarsAsync(userDetailId);
            return Ok(response);
        }

        // ⭐ Toggle Wishlist (Add / Remove)
        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleWishlist(int userDetailId, int carDetailId)
        {
            var response = await _wishlistService.ToggleWishlistAsync(userDetailId, carDetailId);

            return Ok(response);
        }
    }
}

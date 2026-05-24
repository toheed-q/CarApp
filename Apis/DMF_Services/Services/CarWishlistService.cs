using AutoMapper;
using DMF_Services.Data;
using DMF_Services.DTOs.Cars;
using DMF_Services.Helpers;
using DMF_Services.Models;
using DMF_Services.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DMF_Services.Services
{
    public class CarWishlistService : ICarWishlistService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public CarWishlistService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // ⭐ GET WISHLIST CARS
        public async Task<ApiResponse<IEnumerable<CarFilterResultDto>>> GetWishlistCarsAsync(int userDetailId)
        {
            try
            {
                var raw = await _db.Set<CarFilterRaw>()
                        .FromSqlRaw(
                            "EXEC dbo.GetWishlistCarsByUser @UserDetailID",
                            new SqlParameter("@UserDetailID", userDetailId)
                        )
                        .AsNoTracking()
                        .ToListAsync();

                var mappedCars = _mapper.Map<IEnumerable<CarFilterResultDto>>(raw);

                return new ApiResponse<IEnumerable<CarFilterResultDto>>
                {
                    Success = true,
                    Message = "Wishlist cars fetched successfully",
                    Data = mappedCars
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // ⭐ TOGGLE WISHLIST (Add/Remove)
        public Task<ApiResponse<bool>> ToggleWishlistAsync(int userDetailId, int carDetailId)
        {
            try
            {
                var result = _db.Set<WishlistToggleResultDto>()
                        .FromSqlRaw(
                            "EXEC dbo.ToggleCarWishlist @UserDetailID, @CarDetailID",
                            new SqlParameter("@UserDetailID", userDetailId),
                            new SqlParameter("@CarDetailID", carDetailId)
                        )
                        .AsNoTracking()
                        .AsEnumerable()
                        .First();

                return Task.FromResult(new ApiResponse<bool>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.IsWishlisted
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

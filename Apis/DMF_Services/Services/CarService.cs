using AutoMapper;
using DMF_Services.Data;
using DMF_Services.DTOs.Cars;
using DMF_Services.DTOs.Common;
using DMF_Services.Models;
using DMF_Services.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DMF_Services.Services
{
    public class CarService : ICarService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public CarService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CarDetailDto>> GetAllAsync()
        {
            var cars = await _db.CarDetails
                .Include(x => x.CarImage)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<CarDetailDto>>(cars);
        }

        public async Task<CarDetailDto?> GetByIdAsync(int id)
        {
            var car = await _db.CarDetails
                .Include(x => x.CarImage)
                .FirstOrDefaultAsync(x => x.Id == id);

            return car == null ? null : _mapper.Map<CarDetailDto>(car);
        }

        public async Task<PagedResponse<CarFilterResultDto>> GetFilteredCarsAsync(
            string? brand,
            string? model,
            string? search,
            string? fuel,
            string? transmission,
            int owners,
            int priceMore,
            int priceLess,
            int drivenMore,
            int drivenLess,
            int age,
            int userDetailID,
            int dealersID,
            int isActive,
            int page,
            int pageSize,
            string sortBy,
            string sortDir)
        {
            var raw = await _db.Set<CarFilterRaw>()
                .FromSqlRaw(
                    @"EXEC dbo.GetCars 
                    @ByBrand,
                    @ByModel,
                    @BySearch,
                    @ByFuel,
                    @ByTransmission,
                    @ByOwners,
                    @ByPriceMoreThen,
                    @ByPriceLessThen,
                    @ByDrivenMoreThen,
                    @ByDrivenLessThen,
                    @ByAge,
                    @ByDealersID,
                    @ByIsActive,
                    @UserDetailID,
                    @Page,
                    @PageSize,
                    @SortBy,
                    @SortDir",
                    new SqlParameter("@ByBrand", (object?)brand ?? DBNull.Value),
                    new SqlParameter("@ByModel", (object?)model ?? DBNull.Value),
                    new SqlParameter("@BySearch", (object?)search ?? DBNull.Value),
                    new SqlParameter("@ByFuel", (object?)fuel ?? DBNull.Value),
                    new SqlParameter("@ByTransmission", (object?)transmission ?? DBNull.Value),
                    new SqlParameter("@ByOwners", owners),
                    new SqlParameter("@ByPriceMoreThen", priceMore),
                    new SqlParameter("@ByPriceLessThen", priceLess),
                    new SqlParameter("@ByDrivenMoreThen", drivenMore),
                    new SqlParameter("@ByDrivenLessThen", drivenLess),
                    new SqlParameter("@ByAge", age),
                    new SqlParameter("@ByDealersID", dealersID),
                    new SqlParameter("@ByIsActive", isActive),
                    new SqlParameter("@UserDetailID", userDetailID),
                    new SqlParameter("@Page", page),
                    new SqlParameter("@PageSize", pageSize),
                    new SqlParameter("@SortBy", sortBy),
                    new SqlParameter("@SortDir", sortDir)
                )
                .AsNoTracking()
                .ToListAsync();

            var total = raw.FirstOrDefault()?.TotalCount ?? 0;

            return new PagedResponse<CarFilterResultDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                Items = _mapper.Map<IEnumerable<CarFilterResultDto>>(raw)
            };
        }
    }
}

using AutoMapper;
using DMF_Services.Data;
using DMF_Services.DTOs.Cars;
using DMF_Services.DTOs.Common;
using DMF_Services.Models;
using DMF_Services.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

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
            string sortDir,
            double? buyerLat = null,
            double? buyerLon = null)
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
                    @SortDir,
                    @BuyerLat,
                    @BuyerLon",
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
                    new SqlParameter("@SortDir", sortDir),
                    new SqlParameter("@BuyerLat", (object?)buyerLat ?? DBNull.Value),
                    new SqlParameter("@BuyerLon", (object?)buyerLon ?? DBNull.Value)
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
        public async Task<int> CreateCarAsync(CreateCarDto dto)
        {
            var car = new CarDetail
            {
                DealersID = dto.DealersID,
                Brand = dto.Brand,
                Model = dto.Model,
                Price = dto.Price,
                RegistrationNo = dto.RegistrationNo,
                RegistrationDate = dto.RegistrationDate,
                KMDriven = dto.KMDriven,
                Fuel = dto.Fuel,
                Transmission = dto.Transmission,
                IsAccidental = dto.IsAccidental,
                ServiceHistory = dto.ServiceHistory,
                AlloyWheels = dto.AlloyWheels,
                Bluetooth = dto.Bluetooth,
                PowerStaring = dto.PowerStaring,
                PowerWindow = dto.PowerWindow,
                AirBag = dto.AirBag,
                ABS = dto.ABS,
                AirCondition = dto.AirCondition,
                CarLocation = dto.Latitude.HasValue && dto.Longitude.HasValue
                    ? new Point(dto.Longitude.Value, dto.Latitude.Value) { SRID = 4326 }
                    : null
            };

            _db.CarDetails.Add(car);
            await _db.SaveChangesAsync();
            return car.Id;
        }

        public async Task<bool> DeleteCarAsync(int carId)
        {
            var car = await _db.CarDetails.FindAsync(carId);
            if (car == null) return false;
            _db.CarDetails.Remove(car);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task UpdateCarImagesAsync(int carId, List<string> imageUrls)
        {
            var image = await _db.CarImages.FirstOrDefaultAsync(x => x.CarDetailID == carId);

            if (image == null)
            {
                image = new CarImage { CarDetailID = carId };
                _db.CarImages.Add(image);
            }

            var props = typeof(CarImage).GetProperties()
                .Where(p => p.Name.StartsWith("Image") && p.PropertyType == typeof(string))
                .OrderBy(p => p.Name)
                .ToList();

            for (int i = 0; i < props.Count; i++)
                props[i].SetValue(image, i < imageUrls.Count ? imageUrls[i] : null);

            await _db.SaveChangesAsync();
        }
    }
}

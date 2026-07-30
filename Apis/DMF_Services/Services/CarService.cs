using AutoMapper;
using DMF_Services.Data;
using DMF_Services.DTOs.Cars;
using DMF_Services.DTOs.Common;
using DMF_Services.Models;
using DMF_Services.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace DMF_Services.Services
{
    public class CarService : ICarService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<CarService> _logger;

        public CarService(AppDbContext db, IMapper mapper, ILogger<CarService> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
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
            double? buyerLon = null,
            int? cityId = null,
            int? carId = null)
        {
            // -------------------------------------------------------------
            // City and GPS are independent concerns:
            //   * cityId  -> exact filter (@ByCityId), passed straight to the SP.
            //   * GPS      -> distance reference (@BuyerLat/@BuyerLon) for sorting.
            // The GPS values are sanitized here so out-of-range / NaN coordinates
            // can never crash geography::Point inside dbo.GetCars.
            // -------------------------------------------------------------
            (buyerLat, buyerLon) = SanitizeBuyerGps(cityId, buyerLat, buyerLon);

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
                    @BuyerLon,
                    @ByCityId,
                    @ByCarId",
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
                    new SqlParameter("@BuyerLon", (object?)buyerLon ?? DBNull.Value),
                    new SqlParameter("@ByCityId", (object?)cityId ?? DBNull.Value),
                    // Deep-link / share: fetch a single car by id in the exact same
                    // shape as the list (images, location, distance all populated).
                    new SqlParameter("@ByCarId", (object?)carId ?? DBNull.Value)
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
        /// <summary>
        /// Sanitizes the buyer's GPS used by dbo.GetCars (@BuyerLat/@BuyerLon) for
        /// distance sorting. City is intentionally NOT considered here — it is an
        /// independent exact filter (@ByCityId). Out-of-range / NaN / Infinity GPS is
        /// dropped to NULL so geography::Point can never crash. Never throws.
        /// </summary>
        private (double? lat, double? lon) SanitizeBuyerGps(
            int? cityId,
            double? buyerLat,
            double? buyerLon)
        {
            try
            {
                if (buyerLat.HasValue && buyerLon.HasValue)
                {
                    // GPS is untrusted client input — validate before it reaches SQL.
                    if (IsValidCoordinate(buyerLat, buyerLon))
                    {
                        _logger.LogInformation(
                            "Distance reference = buyer GPS=({BuyerLat},{BuyerLon}). " +
                            "CityId={CityId} applied as filter only.",
                            buyerLat, buyerLon, cityId);

                        return (buyerLat, buyerLon);
                    }

                    _logger.LogWarning(
                        "Buyer GPS=({BuyerLat},{BuyerLon}) is out of valid range or non-finite. " +
                        "Dropping to NULL; distance sorting disabled. CityId={CityId}.",
                        buyerLat, buyerLon, cityId);

                    return (null, null);
                }

                _logger.LogInformation(
                    "No buyer GPS supplied. CityId={CityId}. Distance sorting disabled; " +
                    "city filter (if any) still applies.",
                    cityId);

                return (null, null);
            }
            catch (Exception ex)
            {
                // Never break the API response over a GPS-sanitation failure.
                _logger.LogError(ex,
                    "Failed to sanitize buyer GPS=({BuyerLat},{BuyerLon}). Using NULL coordinates. CityId={CityId}.",
                    buyerLat, buyerLon, cityId);

                return (null, null);
            }
        }

        /// <summary>
        /// Validates that a coordinate pair is safe to hand to dbo.GetCars, whose
        /// geography::Point(@BuyerLat, @BuyerLon, 4326) call throws (SQL error 24201/24206)
        /// for out-of-range latitude/longitude and rejects NaN/Infinity. Latitude must be
        /// within [-90, 90] and longitude within [-180, 180].
        /// </summary>
        private static bool IsValidCoordinate(double? lat, double? lon)
        {
            if (!lat.HasValue || !lon.HasValue)
                return false;

            if (double.IsNaN(lat.Value) || double.IsInfinity(lat.Value) ||
                double.IsNaN(lon.Value) || double.IsInfinity(lon.Value))
                return false;

            return lat.Value is >= -90 and <= 90
                && lon.Value is >= -180 and <= 180;
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
                BodyType = dto.BodyType,
                IsNegotiable = dto.IsNegotiable,
                ReverseCamera = dto.ReverseCamera,
                Sunroof = dto.Sunroof,
                CityId = dto.CityId,
                CarLocation = dto.Latitude.HasValue && dto.Longitude.HasValue
                    ? new Point(dto.Longitude.Value, dto.Latitude.Value) { SRID = 4326 }
                    : null
            };

            _db.CarDetails.Add(car);
            await _db.SaveChangesAsync();
            return car.Id;
        }

        public async Task<bool> UpdateCarAsync(int id, CreateCarDto dto)
        {
            var car = await _db.CarDetails.FirstOrDefaultAsync(x => x.Id == id);
            if (car == null) return false;

            car.Brand = dto.Brand;
            car.Model = dto.Model;
            car.Price = dto.Price;
            car.RegistrationNo = dto.RegistrationNo;
            car.RegistrationDate = dto.RegistrationDate;
            car.KMDriven = dto.KMDriven;
            car.Fuel = dto.Fuel;
            car.Transmission = dto.Transmission;
            car.IsAccidental = dto.IsAccidental;
            car.ServiceHistory = dto.ServiceHistory;
            car.AlloyWheels = dto.AlloyWheels;
            car.Bluetooth = dto.Bluetooth;
            car.PowerStaring = dto.PowerStaring;
            car.PowerWindow = dto.PowerWindow;
            car.AirBag = dto.AirBag;
            car.ABS = dto.ABS;
            car.AirCondition = dto.AirCondition;
            car.BodyType = dto.BodyType;
            car.IsNegotiable = dto.IsNegotiable;
            car.ReverseCamera = dto.ReverseCamera;
            car.Sunroof = dto.Sunroof;
            car.CityId = dto.CityId;

            // Only update the location when fresh coordinates were supplied,
            // so editing without GPS permission won't wipe the existing point.
            if (dto.Latitude.HasValue && dto.Longitude.HasValue)
                car.CarLocation = new Point(dto.Longitude.Value, dto.Latitude.Value) { SRID = 4326 };

            await _db.SaveChangesAsync();
            return true;
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

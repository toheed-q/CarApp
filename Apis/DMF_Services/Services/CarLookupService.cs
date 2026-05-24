using AutoMapper;
using DMF_Services.Data;
using DMF_Services.DTOs.Cars;
using DMF_Services.Models;
using DMF_Services.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DMF_Services.Services
{
    public class CarLookupService : ICarLookupService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;

        public CarLookupService(AppDbContext db, IMemoryCache cache, IMapper mapper)
        {
            _db = db;
            _cache = cache;
            _mapper = mapper;
        }

        private const string BrandCacheKey = "CAR_BRANDS";
        private static string ModelCacheKey(string brand) => $"CAR_MODELS_{brand}";

        // -----------------------------
        // Get all brands
        // -----------------------------
        public async Task<IEnumerable<CarBrandDto>> GetBrandsAsync()
        {
            if (_cache.TryGetValue(BrandCacheKey, out IEnumerable<CarBrandDto> cached))
                return cached;

            var raw = await _db.Set<CarBrandRaw>()
                .FromSqlRaw("EXEC dbo.GetCarBrands")
                .AsNoTracking()
                .ToListAsync();

            var result = _mapper.Map<IEnumerable<CarBrandDto>>(raw);

            _cache.Set(BrandCacheKey, result, TimeSpan.FromHours(6));
            return result;
        }

        // -----------------------------
        // Get models by brand
        // -----------------------------
        public async Task<IEnumerable<CarModelDto>> GetModelsByBrandAsync(string brand)
        {
            var key = ModelCacheKey(brand);

            if (_cache.TryGetValue(key, out IEnumerable<CarModelDto> cached))
                return cached;

            var raw = await _db.Set<CarModelRaw>()
                .FromSqlRaw(
                    "EXEC dbo.GetCarModelsByBrand @Brand",
                    new SqlParameter("@Brand", brand))
                .AsNoTracking()
                .ToListAsync();

            var result = _mapper.Map<IEnumerable<CarModelDto>>(raw);

            _cache.Set(key, result, TimeSpan.FromHours(6));
            return result;
        }

        // -----------------------------
        // Combined Brand + Models
        // -----------------------------
        public async Task<IEnumerable<CarBrandWithModelsDto>> GetBrandsWithModelsAsync()
        {
            var brands = await GetBrandsAsync();
            var result = new List<CarBrandWithModelsDto>();

            foreach (var brand in brands)
            {
                var models = await GetModelsByBrandAsync(brand.Brand);

                result.Add(new CarBrandWithModelsDto
                {
                    Brand = brand.Brand,
                    Models = models.Select(m => m.Model).ToList()
                });
            }

            return result;
        }
    }
}

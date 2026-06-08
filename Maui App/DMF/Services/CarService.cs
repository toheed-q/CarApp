using DMF.Helpers;
using DMF.DTOs.Cars;
using System.Text.Json;

namespace DMF.Services
{
    public class CarService : ICarService
    {
        private readonly IApiService _apiService;
        private readonly IBlobService _blobService;

        public CarService(IApiService apiService, IBlobService blobService)
        {
            _apiService = apiService;
            _blobService = blobService;
        }

        public async Task<List<CarModel>> GetCarsAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("cars.json");
            using var reader = new StreamReader(stream);

            var json = await reader.ReadToEndAsync();

            return JsonSerializer.Deserialize<List<CarModel>>(json)
                   ?? new List<CarModel>();
        }

        public async Task<ApiResponse<IEnumerable<CarFilterResult>>> GetFavoriteCarsAsync(int userId)
        {
            return await _apiService.GetAsync<IEnumerable<CarFilterResult>>($"wishlist/{userId}");
        }

        public async Task<ApiResponse<bool>> ToggleWishlistAsync(int userId, int carId)
        {
            try
            {
                var endpoint = $"wishlist/toggle?userDetailId={userId}&carDetailId={carId}";

                return await _apiService.PostAsync<object, bool>(endpoint, new { });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // --------------------------------------------------
        // GET ALL
        // --------------------------------------------------
        public async Task<ApiResponse<IEnumerable<CarModel>>> GetAllCarsAsync()
        {
            return await _apiService
                .GetAsync<IEnumerable<CarModel>>("cars");
        }

        // --------------------------------------------------
        // GET BY ID
        // --------------------------------------------------
        public async Task<ApiResponse<CarModel>> GetCarByIdAsync(int id)
        {
            return await _apiService
                .GetAsync<CarModel>($"cars/{id}");
        }

        // --------------------------------------------------
        // FILTER
        // --------------------------------------------------
        public async Task<ApiResponse<PagedResponse<CarFilterResult>>> GetFilteredCarsAsync(CarFilterModel f)
        {
            var query = new List<string>();

            void Add(string key, string? value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    query.Add($"{key}={Uri.EscapeDataString(value)}");
            }

            void AddInt(string key, int value)
            {
                if (value != 0)
                    query.Add($"{key}={value}");
            }

            void AddDouble(string key, double? value)
            {
                if (value.HasValue)
                    query.Add($"{key}={value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            }

            Add("brand", f.Brand);
            Add("model", f.Model);
            Add("search", f.Search);
            Add("fuel", f.Fuel);
            Add("transmission", f.Transmission);

            AddInt("owners", f.Owners);
            AddInt("priceMore", f.PriceMore);
            AddInt("priceLess", f.PriceLess);
            AddInt("drivenMore", f.DrivenMore);
            AddInt("drivenLess", f.DrivenLess);
            AddInt("age", f.Age);
            AddInt("userDetailID", f.UserDetailID);
            AddInt("dealersID", f.DealersID);

            // always include these
            query.Add($"isActive={f.IsActive}");
            query.Add($"page={f.Page}");
            query.Add($"pageSize={f.PageSize}");
            query.Add($"sortBy={f.SortBy}");
            query.Add($"sortDir={f.SortDir}");

            AddDouble("buyerLat", f.BuyerLat);
            AddDouble("buyerLon", f.BuyerLon);

            System.Diagnostics.Debug.WriteLine($"[Filter] sortBy={f.SortBy} buyerLat={f.BuyerLat} buyerLon={f.BuyerLon}");

            var endpoint = $"cars/filter";

            if (query.Count > 0)
                endpoint += "?" + string.Join("&", query);

            return await _apiService
                .GetAsync<PagedResponse<CarFilterResult>>(endpoint);
        }


        public async Task<ApiResponse<PagedResponse<CarFilterResult>>> GetDealerCarsAsync(int dealersId, int page = 1, int pageSize = 20)
        {
            var endpoint = $"cars/filter?dealersID={dealersId}&isActive=1&page={page}&pageSize={pageSize}&sortBy=price&sortDir=asc";
            return await _apiService.GetAsync<PagedResponse<CarFilterResult>>(endpoint);
        }

        public async Task<ApiResponse<bool>> DeleteCarAsync(int carId)
        {
            return await _apiService.DeleteAsync<bool>($"cars/{carId}");
        }

        public async Task<List<string>?> GetBrandsAsync()
        {
            var response = await _apiService.GetAsync<IEnumerable<CarBrandDto>>("car-lookup/brands");
            return response.Success ? response.Data?.Select(x => x.Brand).ToList() : new List<string>();
        }

        public async Task<List<string>?> GetModelsAsync()
        {
            var response = await _apiService.GetAsync<IEnumerable<CarBrandWithModelsDto>>("car-lookup/brands-models");
            return response.Success
                ? response.Data?.SelectMany(x => x.Models).Distinct().ToList()
                : new List<string>();
        }

        public async Task<ApiResponse<bool>> AddCarAsync(
    AddCarModel model,
    IEnumerable<ImageItem> images,
    string dealerName,
    int dealerId,
    Func<double, Task>? progressCallback = null)
        {
            if (images == null)
                throw new Exception("Images cannot be null.");

            var imageList = images.ToList();

            if (!imageList.Any())
                throw new Exception("At least one image is required.");

            if (imageList.Count > 20)
                throw new Exception("Maximum 20 images allowed.");

            // --------------------------------------
            // STEP 1: Create Car
            // --------------------------------------
            var dto = new
            {
                model.DealersID,
                Brand           = model.Brand,
                Model           = model.Model,
                Price           = model.Price,
                RegistrationNo  = model.RegistrationNo,
                RegistrationDate= model.PurchaseDate.HasValue
                                    ? DateOnly.FromDateTime(model.PurchaseDate.Value)
                                    : (DateOnly?)null,
                KMDriven        = model.OdometerReading,
                Fuel            = model.FuelType,
                Transmission    = model.Transmission,
                IsAccidental    = model.AccidentHistory,
                ServiceHistory  = model.ServiceHistory,
                AlloyWheels     = model.AlloyWheels,
                Bluetooth       = model.Bluetooth,
                PowerStaring    = model.PowerSteering,
                PowerWindow     = model.PowerWindow,
                AirBag          = model.Airbags,
                ABS             = model.ABS,
                AirCondition    = model.AirCondition == true ? "Yes" : model.AirCondition == false ? "No" : (string?)null,
                Latitude        = model.Latitude,
                Longitude       = model.Longitude
            };

            var createResponse = await _apiService.PostAsync<object, int>("cars", dto);

            if (!createResponse.Success || createResponse.Data == null || createResponse.Data == 0)
                throw new Exception(createResponse.Message ?? "Car creation failed.");

            var carId = createResponse.Data;

            // --------------------------------------
            // STEP 2: Upload Images
            // --------------------------------------
            var uploadedUrls = new List<string>();
            var uploadedBlobs = new List<string>();

            var safeDealerName = dealerName
                .Replace(" ", "_")
                .ToLower();

            var dealerFolder = $"{safeDealerName}_{dealerId}";
            var carFolder = $"{carId}";

            int total = imageList.Count;
            int completed = 0;

            try
            {
                var tasks = imageList.Select(async (img, index) =>
                {
                    var extension = Path.GetExtension(img.FilePath);
                    var fileName = $"img_{index + 1}_{Guid.NewGuid():N}{extension}";
                    var blobPath = $"cars/{dealerFolder}/{carFolder}/{fileName}";

                    // ⚠️ IMPORTANT: create stream INSIDE retry
                    var url = await RetryHelper.RetryAsync(async () =>
                    {
                        using var stream = ImageHelper.CompressImage(img.FilePath, 70);
                        return await _blobService.UploadAsync(stream, blobPath, "image/jpeg");
                    });

                    lock (uploadedUrls)
                    {
                        uploadedUrls.Add(url);
                        uploadedBlobs.Add(url);
                    }

                    // 📊 Progress
                    var done = Interlocked.Increment(ref completed);
                    var progress = (double)done / total;

                    if (progressCallback != null)
                        await progressCallback(progress);
                });

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                // ❌ Rollback uploaded blobs
                foreach (var blob in uploadedBlobs)
                {
                    try
                    {
                        await _blobService.DeleteAsync(blob);
                    }
                    catch
                    {
                        // ignore cleanup failure
                    }
                }

                throw new Exception("Image upload failed: " + ex.Message);
            }

            // --------------------------------------
            // STEP 3: Update Images in DB
            // --------------------------------------
            var updateResponse = await _apiService.PutAsync<List<string>, bool>($"cars/{carId}/images", uploadedUrls);

            if (!updateResponse.Success)
                throw new Exception(updateResponse.Message ?? "Failed to update car images.");

            return updateResponse;
        }
    }
}

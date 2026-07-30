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

            // Optional city filter — only sent when a city is actually selected.
            if (f.CityId.HasValue && f.CityId.Value > 0)
                query.Add($"cityId={f.CityId.Value}");

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

        // Fetch one car by id through the same filter path the list uses, so a
        // deep-linked car renders identically (images, location, distance).
        public async Task<CarFilterResult?> GetCarForShareAsync(int carId)
        {
            var endpoint = $"cars/filter?carId={carId}&page=1&pageSize=1";
            var result = await _apiService.GetAsync<PagedResponse<CarFilterResult>>(endpoint);
            return result?.Success == true
                ? result.Data?.Items?.FirstOrDefault()
                : null;
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

        public async Task<List<string>?> GetModelsByBrandAsync(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
                return new List<string>();

            var endpoint = $"car-lookup/models?brand={Uri.EscapeDataString(brand)}";
            var response = await _apiService.GetAsync<IEnumerable<CarModelDto>>(endpoint);
            return response.Success
                ? response.Data?.Select(x => x.Model).ToList() ?? new List<string>()
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
            var createResponse = await _apiService.PostAsync<object, int>("cars", BuildCarPayload(model));

            if (!createResponse.Success || createResponse.Data == null || createResponse.Data == 0)
                throw new Exception(createResponse.Message ?? "Car creation failed.");

            var carId = createResponse.Data;

            // --------------------------------------
            // STEP 2 + 3: Upload Images and save to DB
            // --------------------------------------
            return await UploadAndSaveImagesAsync(carId, imageList, dealerName, dealerId, progressCallback);
        }

        public async Task<ApiResponse<bool>> UpdateCarAsync(
            AddCarModel model,
            IEnumerable<ImageItem> images,
            string dealerName,
            int dealerId,
            Func<double, Task>? progressCallback = null)
        {
            if (model.ID is null or <= 0)
                throw new Exception("A valid car id is required to update.");

            var carId = model.ID.Value;

            // --------------------------------------
            // STEP 1: Update Car fields
            // --------------------------------------
            var updateResponse = await _apiService.PutAsync<object, bool>($"cars/{carId}", BuildCarPayload(model));

            if (!updateResponse.Success)
                throw new Exception(updateResponse.Message ?? "Car update failed.");

            // --------------------------------------
            // STEP 2: Images. The wizard holds both the already-uploaded photos
            // (IsExisting, remote URLs the user kept) and any newly picked local
            // files. We keep the former, upload the latter, and save the merged
            // list — so editing never silently drops the original photos.
            // --------------------------------------
            var usableImages = (images ?? Enumerable.Empty<ImageItem>())
                .Where(i => !string.IsNullOrWhiteSpace(i.FilePath)
                            && (i.IsExisting || File.Exists(i.FilePath)))
                .ToList();

            if (usableImages.Count > 20)
                throw new Exception("Maximum 20 images allowed.");

            // Nothing to persist (user touched no photos / removed everything):
            // leave the car's current images untouched.
            if (!usableImages.Any())
                return updateResponse;

            return await UploadAndSaveImagesAsync(carId, usableImages, dealerName, dealerId, progressCallback);
        }

        // Shared request body for create + update (server maps these fields identically).
        private static object BuildCarPayload(AddCarModel model) => new
        {
            model.DealersID,
            Brand            = model.Brand,
            Model            = model.Model,
            Varient          = model.Varient,
            Price            = model.Price,
            RegistrationNo   = model.RegistrationNo,
            // Prefer an explicit purchase date (set when editing); otherwise derive
            // the registration date from the manufacture year so it is never lost.
            RegistrationDate = model.PurchaseDate.HasValue
                                ? DateOnly.FromDateTime(model.PurchaseDate.Value)
                                : model.YearOfManufacture.HasValue
                                    ? new DateOnly(model.YearOfManufacture.Value, 1, 1)
                                    : (DateOnly?)null,
            KMDriven         = model.OdometerReading,
            Fuel             = model.FuelType,
            Transmission     = model.Transmission,
            IsAccidental     = model.AccidentHistory,
            ServiceHistory   = model.ServiceHistory,
            AlloyWheels      = model.AlloyWheels,
            Bluetooth        = model.Bluetooth,
            PowerStaring     = model.PowerSteering,
            PowerWindow      = model.PowerWindow,
            AirBag           = model.Airbags,
            ABS              = model.ABS,
            AirCondition     = model.AirCondition == true ? "Yes" : model.AirCondition == false ? "No" : (string?)null,
            BodyType         = model.BodyType,
            IsNegotiable     = model.IsNegotiable,
            ReverseCamera    = model.ReverseCamera,
            Sunroof          = model.Sunroof,
            Latitude         = model.Latitude,
            Longitude        = model.Longitude,
            CityId           = model.CityId
        };

        // Uploads any new photos, then saves the full ordered URL list against the
        // car. The primary photo is placed first so it becomes the listing thumbnail
        // (the listing uses images[0]); existing photos keep their blob URLs.
        private async Task<ApiResponse<bool>> UploadAndSaveImagesAsync(
            int carId,
            List<ImageItem> imageList,
            string dealerName,
            int dealerId,
            Func<double, Task>? progressCallback)
        {
            var uploadedBlobs = new List<string>();

            // Map each newly picked photo to the blob URL it gets uploaded to, so we
            // can rebuild the final list in the same on-screen order afterwards.
            var urlByItem = new System.Collections.Concurrent.ConcurrentDictionary<ImageItem, string>();

            // Group blobs by dealer id (stable, rename-proof) then car id:
            // cars/{dealerId}/{carId}/<file>
            var dealerFolder = $"{dealerId}";
            var carFolder = $"{carId}";

            var newItems = imageList.Where(i => !i.IsExisting).ToList();
            int total = newItems.Count;
            int completed = 0;

            try
            {
                var tasks = newItems.Select(async (img, index) =>
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

                    urlByItem[img] = url;
                    lock (uploadedBlobs) uploadedBlobs.Add(url);

                    // 📊 Progress
                    var done = Interlocked.Increment(ref completed);
                    var progress = (double)done / Math.Max(total, 1);

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

            // Rebuild the URL list in on-screen order: existing photos keep their URL,
            // new ones use the freshly uploaded URL. Then float the primary to the
            // front (stable sort keeps everything else in order) so it is images[0].
            var finalUrls = imageList
                .Select(img => new
                {
                    Url = img.IsExisting ? img.FilePath : (urlByItem.TryGetValue(img, out var u) ? u : null),
                    img.IsPrimary
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Url))
                .OrderByDescending(x => x.IsPrimary)
                .Select(x => x.Url!)
                .ToList();

            var saveResponse = await _apiService.PutAsync<List<string>, bool>($"cars/{carId}/images", finalUrls);

            if (!saveResponse.Success)
                throw new Exception(saveResponse.Message ?? "Failed to update car images.");

            return saveResponse;
        }
    }
}

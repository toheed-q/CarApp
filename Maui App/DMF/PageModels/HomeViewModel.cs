using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ViewState = DMF.Enums.ViewState;

namespace DMF.PageModels
{

    public partial class HomeViewModel : ObservableObject
    {
        private CarFilterModel _currentFilter;
        private readonly ICarService _carService;
        private readonly ICityService _cityService;
        private readonly ISecureStorageService _storageService;

        [ObservableProperty]
        private string searchText = string.Empty;

        // Selected location. Together with SearchText this is the combined
        // search+location state, kept here as the single source of truth and
        // mirrored into _currentFilter.CityId for the actual query.
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LocationDisplay))]
        [NotifyPropertyChangedFor(nameof(HasSelectedCity))]
        private DMF.DTOs.Cities.CityDto? selectedCity;

        public string LocationDisplay => SelectedCity?.CityName ?? "All Locations";
        public bool HasSelectedCity => SelectedCity != null;

        private CancellationTokenSource? _searchCts;

        partial void OnSearchTextChanged(string value)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;
            Task.Delay(500, token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                _currentFilter.Search = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                MainThread.BeginInvokeOnMainThread(() => LoadCarsCommand.Execute(null));
            }, token);
        }

        [ObservableProperty]
        private ObservableCollection<CarFilterResult> _cars;

        [ObservableProperty]
        private bool isLoadingMore;

        [ObservableProperty]
        private bool canLoadMore;


        private bool _hasMoreData = true;
        private int _totalRecords;

        [ObservableProperty]
        private ViewState currentState = ViewState.Loading;

        [ObservableProperty]
        private string emptyMessage = string.Empty;



        public HomeViewModel(ICarService carService, ICityService cityService, ISecureStorageService secureStorage)
        {
            CurrentState = ViewState.Loading;
            _carService = carService;
            _cityService = cityService;
            _storageService = secureStorage;
            _cars = new ObservableCollection<CarFilterResult>();
            _currentFilter = new CarFilterModel();

            var userIdTask = _storageService.GetAsync(AppConstants.UserId);
            userIdTask.Wait();
            var userIdString = userIdTask.Result;
            if (int.TryParse(userIdString, out var userId))
            {
                _currentFilter.UserDetailID = userId;
            }
            else
            {
                _currentFilter.UserDetailID = 0;
            }
        }

        private bool _initialized = false;
        private int _loadedUserId = int.MinValue;

        public void Initialize() => _ = InitializeAsync();

        // Load the home for the current user, then open any car queued from a share
        // link opened while the app was starting (cold-start deep link).
        private async Task InitializeAsync()
        {
            bool wasInitialized = _initialized;

            await EnsureLoadedForCurrentUserAsync();

            // Returning to Home from another page should start fresh: if any filter,
            // search or location is still applied, clear it and reload so a stale
            // filtered list never sticks. (The very first load is already unfiltered.)
            if (wasInitialized && HasActiveFilters())
            {
                ResetFilters();
                await LoadCars();
            }

            await DMF.Helpers.DeepLinkHandler.ConsumePendingAsync();
        }

        private bool HasActiveFilters()
            => !string.IsNullOrWhiteSpace(_currentFilter.Brand)
            || !string.IsNullOrWhiteSpace(_currentFilter.Model)
            || !string.IsNullOrWhiteSpace(_currentFilter.Fuel)
            || !string.IsNullOrWhiteSpace(_currentFilter.Search)
            || _currentFilter.PriceMore != 0 || _currentFilter.PriceLess != 0
            || _currentFilter.DrivenMore != 0 || _currentFilter.DrivenLess != 0
            || _currentFilter.Age != 0 || _currentFilter.Owners != 0
            || (_currentFilter.CityId.HasValue && _currentFilter.CityId.Value > 0)
            || _currentFilter.SortBy != "price" || _currentFilter.SortDir != "asc";

        // Clears every applied filter/search/location back to defaults (mirrors the
        // filter sheet's "Clear All").
        private void ResetFilters()
        {
            _currentFilter.Brand      = null;
            _currentFilter.Model      = null;
            _currentFilter.Fuel       = null;
            _currentFilter.Search     = null;
            _currentFilter.PriceMore  = 0;
            _currentFilter.PriceLess  = 0;
            _currentFilter.DrivenMore = 0;
            _currentFilter.DrivenLess = 0;
            _currentFilter.Age        = 0;
            _currentFilter.Owners     = 0;
            _currentFilter.CityId     = null;
            _currentFilter.SortBy     = "price";
            _currentFilter.SortDir    = "asc";

            // Reset the on-screen search box + location chip. Cancel any pending
            // debounced search reload so it can't fire a second load.
            SearchText = string.Empty;
            SelectedCity = null;
            _searchCts?.Cancel();
        }

        // Loads or reloads the home for the CURRENTLY signed-in user. The Home view
        // is cached inside the MainPage singleton, so this must re-check the user on
        // each appearance — otherwise a re-login keeps showing the previous user's
        // wishlist hearts. A full reload happens only on first run or when the user
        // actually changed, so simply revisiting the tab preserves scroll/filters.
        public async Task EnsureLoadedForCurrentUserAsync()
        {
            var idStr = await _storageService.GetAsync(AppConstants.UserId);
            int.TryParse(idStr, out var uid);

            if (_initialized && uid == _loadedUserId)
                return;

            bool firstRun = !_initialized;
            _initialized = true;
            _loadedUserId = uid;
            _currentFilter.UserDetailID = uid;

            if (firstRun)
                await LoadBuyerLocationAsync();

            await LoadCars();
        }

        [RelayCommand]
        private async Task LoadCars()
        {
            CurrentState = ViewState.Loading;
            IsLoadingMore = false; // reset stuck state

            Cars.Clear();
            _currentFilter.Page = 1;
            _currentFilter.PageSize = 20;

            _hasMoreData = true;
            _totalRecords = 0;

            // Retry to ride out the Azure serverless cold-start: the first request
            // after the DB has auto-paused can fail/time out while it wakes, and we
            // must not mislabel that failure as "no cars found".
            bool ok = false;
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                ok = await LoadNextPage();
                if (ok) break;
                if (attempt < 3)
                    await Task.Delay(TimeSpan.FromSeconds(3));
            }

            if (!ok)
            {
                // Genuine failure (server unreachable / waking up) — not an empty filter.
                EmptyMessage = "We couldn't reach the server. Pull down or tap retry.";
                CurrentState = ViewState.Error;
            }
            else if (Cars.Count == 0)
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(_currentFilter.Brand))  parts.Add(_currentFilter.Brand);
                if (!string.IsNullOrWhiteSpace(_currentFilter.Model))  parts.Add(_currentFilter.Model);
                if (!string.IsNullOrWhiteSpace(_currentFilter.Search)) parts.Add(_currentFilter.Search);
                EmptyMessage = parts.Count > 0 ? $"for {string.Join(" ", parts)}" : "for the applied filters";
                CurrentState = ViewState.Empty;
            }
            else
            {
                CurrentState = ViewState.Success;
            }
        }

        [RelayCommand]
        private async Task LoadMoreCars()
        {
            Debug.WriteLine($"LoadMore fired — Cars:{Cars.Count}");

            if (!CanLoadMore)
                return;

            if (IsLoadingMore)
                return;

            if (!_hasMoreData)
                return;

            if (_totalRecords == 0)
                return;

            await LoadNextPage();
        }


        [RelayCommand] async Task Filter()
        {
            var brands = await _carService.GetBrandsAsync();
            var models = await _carService.GetModelsAsync();
            var popup = new DMF.Pages.Popups.FilterPopup(
                brands ?? new List<string>(),
                models ?? new List<string>());

            var result = await Application.Current!.Windows[0].Page!.ShowPopupAsync(popup) as DMF.Pages.Popups.FilterResult;
            if (result == null) return;

            // If cleared, reset filter and reload
            if (result.IsCleared)
            {
                _currentFilter.Brand      = null;
                _currentFilter.Model      = null;
                _currentFilter.Fuel       = null;
                _currentFilter.PriceMore  = 0;
                _currentFilter.PriceLess  = 0;
                _currentFilter.DrivenMore = 0;
                _currentFilter.DrivenLess = 0;
                _currentFilter.Age        = 0;
                _currentFilter.Owners     = 0;
                _currentFilter.SortBy     = "price";
                _currentFilter.SortDir    = "asc";
                await LoadCars();
                return;
            }

            _currentFilter.Brand        = result.Brand;
            _currentFilter.Model        = result.Model;
            _currentFilter.Fuel         = result.Fuel;
            _currentFilter.PriceMore    = result.MinPrice;
            _currentFilter.PriceLess    = result.MaxPrice;
            _currentFilter.DrivenMore   = result.MinKm;
            _currentFilter.DrivenLess   = result.MaxKm;
            _currentFilter.Age          = result.Age;
            _currentFilter.Owners       = result.Owners;
            _currentFilter.SortBy       = result.SortBy;
            _currentFilter.SortDir      = result.SortDir;

            await LoadCars();
        }
        // ── Location selector ─────────────────────────────────────────────
        [RelayCommand]
        async Task SelectLocation()
        {
            var popup = new DMF.Pages.Popups.CitySelectionPopup(_cityService);

            var result = await Application.Current!.Windows[0].Page!
                .ShowPopupAsync(popup) as DMF.Pages.Popups.CitySelectionResult;

            // null => user cancelled; keep the previous selection untouched.
            if (result == null) return;

            if (result.IsCleared)
            {
                SelectedCity = null;
                _currentFilter.CityId = null;
            }
            else
            {
                SelectedCity = result.City;
                _currentFilter.CityId = result.City?.Id;
            }

            await LoadCars();
        }

        [RelayCommand]
        async Task ClearLocation()
        {
            if (SelectedCity == null) return;

            SelectedCity = null;
            _currentFilter.CityId = null;

            await LoadCars();
        }

        [RelayCommand] async Task Sort()
        {
            var brands = await _carService.GetBrandsAsync();
            var models = await _carService.GetModelsAsync();
            var popup = new DMF.Pages.Popups.FilterPopup(
                brands ?? new List<string>(),
                models ?? new List<string>(),
                initialPanel: "sort");

            var result = await Application.Current!.Windows[0].Page!.ShowPopupAsync(popup) as DMF.Pages.Popups.FilterResult;
            if (result == null) return;

            if (result.IsCleared)
            {
                _currentFilter.SortBy  = "price";
                _currentFilter.SortDir = "asc";
                await LoadCars();
                return;
            }

            _currentFilter.SortBy  = result.SortBy;
            _currentFilter.SortDir = result.SortDir;

            // Sorting by distance always re-reads the device's current location
            // so the ordering reflects where the user is right now.
            if (result.SortBy == "distance")
            {
                var located = await LoadBuyerLocationAsync();
                if (!located)
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Location needed",
                        "Turn on location and allow permission to sort cars by distance.",
                        "OK");
                }
            }

            await LoadCars();
        }
        [RelayCommand] async Task Brand()
        {
            var brands = await _carService.GetBrandsAsync();
            var models = await _carService.GetModelsAsync();
            var popup = new DMF.Pages.Popups.FilterPopup(
                brands ?? new List<string>(),
                models ?? new List<string>(),
                initialPanel: "brand");

            var result = await Application.Current!.Windows[0].Page!.ShowPopupAsync(popup) as DMF.Pages.Popups.FilterResult;
            if (result == null) return;

            if (result.IsCleared)
            {
                _currentFilter.Brand = null;
                _currentFilter.Model = null;
                await LoadCars();
                return;
            }

            _currentFilter.Brand = result.Brand;
            _currentFilter.Model = result.Model;
            await LoadCars();
        }

        [RelayCommand] async Task Model()
        {
            var brands = await _carService.GetBrandsAsync();
            var models = await _carService.GetModelsAsync();
            var popup = new DMF.Pages.Popups.FilterPopup(
                brands ?? new List<string>(),
                models ?? new List<string>(),
                initialPanel: "brand");

            var result = await Application.Current!.Windows[0].Page!.ShowPopupAsync(popup) as DMF.Pages.Popups.FilterResult;
            if (result == null) return;

            if (result.IsCleared)
            {
                _currentFilter.Brand = null;
                _currentFilter.Model = null;
                await LoadCars();
                return;
            }

            _currentFilter.Brand = result.Brand;
            _currentFilter.Model = result.Model;
            await LoadCars();
        }

        [RelayCommand]
        async Task CarDetail(CarFilterResult model)
        {
            await Shell.Current.GoToAsync("cardetails", new Dictionary<string, object>
            {
                {"carDetail", model   }
            });
        }

        [RelayCommand]
        async Task LikeUnlike(CarFilterResult model)
        {
            if (model == null) return;

            // Use the currently logged-in user, read fresh so re-login is reflected.
            var idStr = await _storageService.GetAsync(AppConstants.UserId);
            if (!int.TryParse(idStr, out var userId) || userId <= 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Sign in required", "Please sign in to save cars to your wishlist.", "OK");
                return;
            }

            var response = await _carService.ToggleWishlistAsync(userId, model.ID);

            if (response.Success)
            {
                // response.Data is the new wishlisted state for this car
                model.IsWishlisted = response.Data;
            }
        }

        // Fetches the device's CURRENT location via the OS location service and
        // stores it as the distance reference. Returns true only when a fresh
        // (or last-known) fix was obtained. Used by "Sort by distance".
        private async Task<bool> LoadBuyerLocationAsync()
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    Debug.WriteLine("[BuyerLocation] Permission denied");
                    return false;
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)), cts.Token);

                // Fall back to the last known fix only if a live one isn't available.
                location ??= await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    _currentFilter.BuyerLat = location.Latitude;
                    _currentFilter.BuyerLon = location.Longitude;
                    Debug.WriteLine($"[BuyerLocation] Set: {location.Latitude}, {location.Longitude}");
                    return true;
                }

                Debug.WriteLine("[BuyerLocation] No location obtained");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BuyerLocation] Failed: {ex.Message}");
                return false;
            }
        }

        // Returns true when the request succeeded (even if it matched zero cars),
        // false when it failed (network/timeout/server error). The caller uses this
        // to tell a genuine empty result apart from a load failure.
        private async Task<bool> LoadNextPage()
        {
            if (IsLoadingMore)
                return true;

            try
            {
                IsLoadingMore = true;

                var result = await _carService.GetFilteredCarsAsync(_currentFilter);

                // A failed envelope or missing payload is a failure, not an empty result.
                if (result == null || !result.Success || result.Data == null)
                    return false;

                var page = result.Data;

                _totalRecords = page.TotalRecords;
                if (Cars.Count == 0)
                    Cars = new ObservableCollection<CarFilterResult>(page.Items);
                else
                    foreach (var car in page.Items)
                        Cars.Add(car);

                _currentFilter.Page++;

                _hasMoreData = Cars.Count < _totalRecords;
                CanLoadMore = Cars.Count >= _currentFilter.PageSize;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadNextPage] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"[LoadNextPage] StackTrace: {ex.StackTrace}");
                return false;
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

    }
}
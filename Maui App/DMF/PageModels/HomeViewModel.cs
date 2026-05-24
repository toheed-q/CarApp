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
        private readonly ISecureStorageService _storageService;

        [ObservableProperty]
        private string searchText = string.Empty;

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



        public HomeViewModel(ICarService carService, ISecureStorageService secureStorage)
        {
            CurrentState = ViewState.Loading;
            _carService = carService;
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

        public void Initialize()
        {
            LoadCarsCommand.Execute(null);
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

            await LoadNextPage();
            CurrentState = ViewState.Success;
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
        [RelayCommand] void Sort() { }
        [RelayCommand] void Brand() { }
        [RelayCommand] void Model() { }

        [RelayCommand]
        void CarDetail(CarFilterResult model)
        {
            Shell.Current.GoToAsync("cardetails", new Dictionary<string, object>
            {
                {"carDetail", model   }
            });
        }

        [RelayCommand]
        async Task LikeUnlike(CarFilterResult model)
        {
            try
            {
                var response = await _carService.ToggleWishlistAsync(7, model.ID);

                if (response.Success)
                {
                    var car = Cars.FirstOrDefault(x => x.ID == model.ID);
                    if (car != null)
                        car.IsWishlisted = response.Data;
                    OnPropertyChanged("Cars");
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async Task LoadNextPage()
        {
            if (IsLoadingMore)
                return;

            try
            {
                IsLoadingMore = true;

                var result = await _carService.GetFilteredCarsAsync(_currentFilter);

                var page = result.Data;
                if (page == null)
                    return;

                _totalRecords = page.TotalRecords;
                if (Cars.Count == 0)
                    Cars = new ObservableCollection<CarFilterResult>(page.Items);
                else
                    foreach (var car in page.Items)
                        Cars.Add(car);

                _currentFilter.Page++;

                _hasMoreData = Cars.Count < _totalRecords;
                CanLoadMore = Cars.Count >= _currentFilter.PageSize;
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

    }
}

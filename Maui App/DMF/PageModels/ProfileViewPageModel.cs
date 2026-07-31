using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.DTOs.User;
using DMF.Utilities;
using System.Collections.ObjectModel;
using ViewState = DMF.Enums.ViewState;

namespace DMF.PageModels
{
    public partial class ProfileViewPageModel : ObservableObject, IQueryAttributable
    {
        private readonly ICarService _carService;
        private readonly IUserDetailService _userDetailService;
        private readonly ISecureStorageService _storage;

        [ObservableProperty] private bool isLoanSelected = false;
        [ObservableProperty] private bool isRegistrationSelected = false;
        [ObservableProperty] private bool isNocSelected = false;
        [ObservableProperty] private ObservableCollection<CarFilterResult> cars = new();
        [ObservableProperty] private ViewState currentState = ViewState.Loading;
        [ObservableProperty] private string userName = string.Empty;
        [ObservableProperty] private string userMobile = string.Empty;
        [ObservableProperty] private string userCity = string.Empty;
        [ObservableProperty] private bool isReadOnly = false;

        // Raw ProfileImage value (URL or placeholder token) resolved by the converter.
        [ObservableProperty] private string? profileImage;

        // True only when the signed-in user is a dealer — controls the Add Car button.
        [ObservableProperty] private bool isDealer = false;

        public string PortfolioHeading => IsReadOnly ? "Their Portfolio" : "Your Portfolio";
        partial void OnIsReadOnlyChanged(bool value) => OnPropertyChanged(nameof(PortfolioHeading));

        private int _dealerId;
        private UserDetailDto? _cachedUser;

        public ProfileViewPageModel(ICarService carService, IUserDetailService userDetailService, ISecureStorageService storage)
        {
            _carService = carService;
            _userDetailService = userDetailService;
            _storage = storage;
        }

        // Shell calls this after all query params are set — replaces Loaded event
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("dealerId", out var val) &&
                int.TryParse(val?.ToString(), out var dealerId) &&
                dealerId > 0)
            {
                _ = InitializeForDealerAsync(dealerId);
            }
            else
            {
                _ = InitializeAsync();
            }
        }

        // Own profile (editable)
        public async Task InitializeAsync()
        {
            var idStr = await _storage.GetAsync(AppConstants.DealersId);
            int.TryParse(idStr, out _dealerId);

            IsReadOnly = false;
            await LoadProfileAsync(_dealerId);

            // Dealer flag from the freshly loaded profile (falls back to stored value).
            if (_cachedUser != null)
            {
                IsDealer = _cachedUser.IsDealers;
            }
            else
            {
                var flag = await _storage.GetAsync(AppConstants.IsDealers);
                IsDealer = bool.TryParse(flag, out var d) && d;
            }

            await LoadCars(_dealerId);
        }

        // Another dealer's profile (read-only)
        public async Task InitializeForDealerAsync(int dealerId)
        {
            var myIdStr = await _storage.GetAsync(AppConstants.DealersId);
            int.TryParse(myIdStr, out _dealerId);

            IsReadOnly = true;
            await LoadProfileAsync(dealerId);
            await LoadCars(dealerId);
        }

        private async Task LoadProfileAsync(int dealerId)
        {
            if (dealerId <= 0)
            {
                UserName = "Guest";
                UserCity = string.Empty;
                return;
            }

            var userResult = await _userDetailService.GetByIdAsync(dealerId);
            if (userResult?.Data != null)
            {
                _cachedUser = userResult.Data;
                UserName = _cachedUser.CompanyName ?? _cachedUser.FirstName;
                UserCity = _cachedUser.City ?? string.Empty;
                ProfileImage = _cachedUser.ProfileImage;
                IsLoanSelected         = _cachedUser.LoanService         == true;
                IsRegistrationSelected = _cachedUser.RegistrationService == true;
                IsNocSelected          = _cachedUser.NocService          == true;
            }
        }

        [RelayCommand] Task Back() => Shell.Current.GoToAsync("..", true);

        [RelayCommand]
        async Task SelectService(string param)
        {
            if (IsReadOnly) return;

            switch (param)
            {
                case "Loan":         IsLoanSelected         = !IsLoanSelected;         break;
                case "Registration": IsRegistrationSelected = !IsRegistrationSelected; break;
                case "NOC":          IsNocSelected          = !IsNocSelected;          break;
            }

            if (_cachedUser == null || _dealerId <= 0) return;

            _cachedUser.LoanService         = IsLoanSelected;
            _cachedUser.RegistrationService = IsRegistrationSelected;
            _cachedUser.NocService          = IsNocSelected;

            await _userDetailService.UpdateAsync(_dealerId, _cachedUser);
        }

        private async Task LoadCars(int dealerId)
        {
            CurrentState = ViewState.Loading;

            if (dealerId <= 0)
            {
                Cars = new ObservableCollection<CarFilterResult>();
                CurrentState = ViewState.Success;
                return;
            }

            var result = await _carService.GetDealerCarsAsync(dealerId);

            var items = result?.Data?.Items;
            if (items != null)
            {
                // Edit/Delete only on your own portfolio AND only while you are a dealer.
                // A user demoted from dealer keeps their old listings but can no longer manage them.
                foreach (var car in items)
                    car.CanManage = !IsReadOnly && IsDealer;

                Cars = new ObservableCollection<CarFilterResult>(items);
            }
            else
            {
                Cars = new ObservableCollection<CarFilterResult>();
            }

            CurrentState = ViewState.Success;
        }

        // Opens the same car detail page used from the home screen — for any viewer.
        [RelayCommand]
        async Task OpenCar(CarFilterResult car)
        {
            if (car == null) return;
            await Shell.Current.GoToAsync("cardetails", new Dictionary<string, object>
            {
                { "carDetail", car }
            });
        }

        [RelayCommand]
        async Task EditCar(CarFilterResult car)
        {
            if (IsReadOnly || !IsDealer) return;
            await Shell.Current.GoToAsync("AddCarStep1", new Dictionary<string, object>
            {
                { "editCarId", car.ID }
            });
        }

        [RelayCommand]
        async Task DeleteCar(CarFilterResult car)
        {
            if (IsReadOnly || !IsDealer) return;

            bool confirm = await Shell.Current.CurrentPage.DisplayAlert(
                "Delete Car",
                $"Are you sure you want to delete {car.Brand} {car.Model}?",
                "Yes", "No");

            if (!confirm) return;

            var result = await _carService.DeleteCarAsync(car.ID);
            if (result.Success)
                Cars.Remove(car);
            else
                await Shell.Current.CurrentPage.DisplayAlert("Error", result.Message ?? "Delete failed", "OK");
        }

        [RelayCommand]
        Task EditProfile() => Shell.Current.GoToAsync("editprofile");

        [RelayCommand]
        public async void NavigateToHome()
        {
            // Land on the Home tab, not whatever tab the singleton MainPage last showed.
            MainPageModel.ForceHomeOnAppear = true;
            await Shell.Current.GoToAsync("///mainPage");
        }

        [RelayCommand]
        public void NavigateToAddCar()
        {
            // Only dealers may add cars; non-dealers can browse only.
            if (!IsDealer) return;
            Shell.Current.GoToAsync("AddCarStep1");
        }
    }
}

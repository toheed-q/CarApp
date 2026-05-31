using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.Utilities;
using System.Collections.ObjectModel;
using ViewState = DMF.Enums.ViewState;

namespace DMF.PageModels
{
    public partial class ProfileViewPageModel : ObservableObject
    {
        private readonly ICarService _carService;
        private readonly ISecureStorageService _storage;

        [ObservableProperty] private bool isLoanSelected = false;
        [ObservableProperty] private bool isRegistrationSelected = false;
        [ObservableProperty] private bool isNocSelected = false;
        [ObservableProperty] private ObservableCollection<CarFilterResult> _cars;
        [ObservableProperty] private ViewState currentState = ViewState.Loading;
        [ObservableProperty] private string userName = string.Empty;
        [ObservableProperty] private string userMobile = string.Empty;
        [ObservableProperty] private string userCity = string.Empty;

        public ProfileViewPageModel(ICarService carService, ISecureStorageService storage)
        {
            _carService = carService;
            _storage = storage;
            CurrentState = new ViewState();
            Cars = new ObservableCollection<CarFilterResult>();
        }

        public async Task InitializeAsync()
        {
            UserName = await _storage.GetAsync(AppConstants.UserName) ?? "Guest";
            UserMobile = await _storage.GetAsync(AppConstants.UserMobile) ?? string.Empty;
            await LoadCarsCommand.ExecuteAsync(null);
        }

        [RelayCommand] Task Back() => Shell.Current.GoToAsync("..", true);

        [RelayCommand]
        void SelectService(string param)
        {
            switch (param)
            {
                case "Loan":
                    IsLoanSelected = !IsLoanSelected;
                    break;
                case "Registration":
                    IsRegistrationSelected = !IsRegistrationSelected;
                    break;
                case "NOC":
                    IsNocSelected = !IsNocSelected;
                    break;
            }
        }

        [RelayCommand]
        private async Task LoadCars()
        {
            CurrentState = ViewState.Loading;

            var result = await _carService.GetFavoriteCarsAsync(7);

            var page = result.Data;
            if (page == null)
                return;
            Cars = new ObservableCollection<CarFilterResult>(page);

            CurrentState = ViewState.Success;
        }

        [RelayCommand]
        public async void NavigateToHome()
        {
            await Shell.Current.GoToAsync("///mainPage");
        }

        [RelayCommand]
        public void NavigateToAddCar()
        {
            Shell.Current.GoToAsync("AddCarStep1", new Dictionary<string, object>
            {

            });
        }
    }
}

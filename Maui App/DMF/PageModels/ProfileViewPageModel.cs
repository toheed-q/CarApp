using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ViewState = DMF.Enums.ViewState;

namespace DMF.PageModels
{
    public partial class ProfileViewPageModel : ObservableObject
    {
        private readonly ICarService _carService;

        [ObservableProperty]
        private bool isLoanSelected = false;

        [ObservableProperty]
        private bool isRegistrationSelected = false;

        [ObservableProperty]
        private bool isNocSelected = false;

        [ObservableProperty]
        private ObservableCollection<CarFilterResult> _cars;

        [ObservableProperty]
        private ViewState currentState = ViewState.Loading;

        public ProfileViewPageModel(ICarService carService)
        {
            _carService = carService;
            CurrentState = new ViewState();
            Cars = new ObservableCollection<CarFilterResult>();
        }

        public void Initialize()
        {
            LoadCarsCommand.Execute(null);
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

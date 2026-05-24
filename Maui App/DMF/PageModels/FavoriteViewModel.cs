using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ViewState = DMF.Enums.ViewState;

namespace DMF.PageModels
{
    public partial class FavoriteViewModel : ObservableObject
    {
        private readonly ICarService _carService;

        [ObservableProperty]
        private ObservableCollection<CarFilterResult> _cars;

        [ObservableProperty]
        private ViewState currentState = ViewState.Loading;

        public FavoriteViewModel(ICarService carService)
        {
            CurrentState = new ViewState();
            _carService = carService;
            Cars = new ObservableCollection<CarFilterResult>();
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

        public void Initialize()
        {
            LoadCarsCommand.Execute(null);
        }

        [RelayCommand]
        void LikeUnlike(CarModel model)
        {

        }

        [RelayCommand]
        void CarDetail(CarFilterResult model)
        {
            Shell.Current.GoToAsync("cardetails", new Dictionary<string, object>
            {
                {"carDetail", model   }
            });
        }
    }
}

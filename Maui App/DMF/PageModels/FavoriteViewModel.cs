using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DMF.Messages;
using DMF.Utilities;
using System.Collections.ObjectModel;
using ViewState = DMF.Enums.ViewState;

namespace DMF.PageModels
{
    public partial class FavoriteViewModel : ObservableObject
    {
        private readonly ICarService _carService;
        private readonly ISecureStorageService _storage;

        [ObservableProperty]
        private ObservableCollection<CarFilterResult> _cars;

        [ObservableProperty]
        private ViewState currentState = ViewState.Loading;

        public FavoriteViewModel(ICarService carService, ISecureStorageService storage)
        {
            CurrentState = new ViewState();
            _carService = carService;
            _storage = storage;
            Cars = new ObservableCollection<CarFilterResult>();
        }

        [RelayCommand]
        private async Task LoadCars()
        {
            CurrentState = ViewState.Loading;

            var idStr = await _storage.GetAsync(AppConstants.UserId);
            if (!int.TryParse(idStr, out var userId) || userId <= 0)
            {
                Cars = new ObservableCollection<CarFilterResult>();
                CurrentState = ViewState.Success;
                return;
            }

            var result = await _carService.GetFavoriteCarsAsync(userId);

            var page = result.Data;
            // Everything in this list is a favorite, so show a filled heart on each.
            if (page != null)
                foreach (var c in page)
                    c.IsWishlisted = true;

            Cars = page != null
                ? new ObservableCollection<CarFilterResult>(page)
                : new ObservableCollection<CarFilterResult>();

            CurrentState = ViewState.Success;
        }

        public void Initialize()
        {
            LoadCarsCommand.Execute(null);
        }

        // Tapping the heart in the wishlist removes the car from it.
        [RelayCommand]
        async Task LikeUnlike(CarFilterResult model)
        {
            if (model == null) return;

            var idStr = await _storage.GetAsync(AppConstants.UserId);
            if (!int.TryParse(idStr, out var userId) || userId <= 0)
                return;

            var response = await _carService.ToggleWishlistAsync(userId, model.ID);
            if (response.Success)
            {
                Cars.Remove(model);
                // Tell the Home / Detail screens so the heart clears there too.
                WeakReferenceMessenger.Default.Send(new WishlistChangedMessage(model.ID, false));
            }
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

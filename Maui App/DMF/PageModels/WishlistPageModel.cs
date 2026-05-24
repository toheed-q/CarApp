using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMF.PageModels
{
    public partial class WishlistPageModel : ObservableObject
    {
        private readonly FavoriteView _favoriteView;

        [ObservableProperty]
        private View currentView = null!;

        public WishlistPageModel(FavoriteViewModel favoriteViewModel)
        {
            _favoriteView = new FavoriteView(favoriteViewModel);
        }

        public void Initialize()
        {
            CurrentView = _favoriteView;
        }

        [RelayCommand] Task Back() => Shell.Current.GoToAsync("..", true);
    }
}

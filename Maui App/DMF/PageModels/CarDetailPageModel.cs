using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMF.PageModels
{
    public partial class CarDetailPageModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty]
        private CarFilterResult carDetail;

        [ObservableProperty]
        private int currentImageIndex;

        [ObservableProperty] private bool isFavorite;

        public string ImageCounter =>
            $"{CurrentImageIndex + 1}/{CarDetail?.Images?.Count()}";
        public string FavoriteIcon => IsFavorite ? "ic_heart_filled.png" : "ic_heart.png";

        partial void OnCurrentImageIndexChanged(int value)
        {
            OnPropertyChanged(nameof(ImageCounter));
        }

        public CarDetailPageModel()
        {
            carDetail = new CarFilterResult();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("carDetail", out var car))
            {
                CarDetail = (CarFilterResult)car ?? new CarFilterResult();
                OnPropertyChanged(nameof(ImageCounter));
            }
        }

        [RelayCommand] Task Back() => Shell.Current.GoToAsync("..");

        [RelayCommand]
        private void Share()
        {
            // Share logic
        }

        [RelayCommand]
        private void Favorite()
        {
            IsFavorite = !IsFavorite;
        }

        [RelayCommand]
        private void NextImage()
        {
            if (CurrentImageIndex < CarDetail?.Images.Count - 1)
                CurrentImageIndex++;
        }

        [RelayCommand]
        private void PreviousImage()
        {
            if (CurrentImageIndex > 0)
                CurrentImageIndex--;
        }

        [RelayCommand]
        private void ViewUserProfile()
        {
            Shell.Current.GoToAsync("profile", new Dictionary<string, object>
            {

            });
        }
    }
}

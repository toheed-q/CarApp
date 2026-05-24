using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMF.PageModels
{
    public partial class AccountViewModel : ObservableObject
    {
        public AccountViewModel() { }

        [RelayCommand]
        void Logout()
        {
            // Implement logout logic here
        }

        [RelayCommand]
        void ViewProfile()
        {
            // Implement view profile logic here    

        }

        [RelayCommand]
        void ContactSupport()
        {
            Shell.Current.GoToAsync("contactus");
        }

        [RelayCommand]
        void BuyPackages()
        {
            // Implement buy packages logic here
        }
        [RelayCommand]
        void JoinAsSeller()
        {
            // Implement join as seller logic here
        }
        [RelayCommand]
        void ViewWishlist()
        {
            Shell.Current.GoToAsync("wishlist");
        }
    }
}

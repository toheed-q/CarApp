using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMF.PageModels
{
    public partial class GetStartedPageModel : ObservableObject
    {
        [RelayCommand]
        private async void NavigateToHomePage()
        {
            string? oauthToken = await SecureStorage.Default.GetAsync(AppConstants.OauthToken);
            if (oauthToken == null)
            {
                await Shell.Current.GoToAsync("///login");
                //await Shell.Current.GoToAsync("///mainPage");
            }
            else
            {
                await Shell.Current.GoToAsync("///mainPage");
            }
        }
    }
}

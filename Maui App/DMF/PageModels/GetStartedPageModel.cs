using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.Constants;

namespace DMF.PageModels
{
    public partial class GetStartedPageModel : ObservableObject
    {
        [RelayCommand]
        private async void NavigateToHomePage()
        {
            // Mark get started as seen
            await SecureStorage.Default.SetAsync(AppConstants.GetStarted, "true");

            // Check the correct token key
            string? authToken = await SecureStorage.Default.GetAsync(AppKeys.AuthToken);
            if (string.IsNullOrEmpty(authToken))
            {
                await Shell.Current.GoToAsync("///login");
            }
            else
            {
                MainPageModel.ForceHomeOnAppear = true;
                await Shell.Current.GoToAsync("///mainPage");
            }
        }

        [RelayCommand]
        private async Task NavigateToSignIn()
        {
            await Shell.Current.GoToAsync("///signin");
        }
    }
}

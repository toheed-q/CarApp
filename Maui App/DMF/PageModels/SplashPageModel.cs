using CommunityToolkit.Mvvm.ComponentModel;

namespace DMF.PageModels
{
    public partial class SplashPageModel : ObservableObject
    {
        public SplashPageModel()
        {
        }

        public async void NavigateToHomePage()
        {
            string? getStarted = await SecureStorage.Default.GetAsync(AppConstants.GetStarted);
            if (getStarted == null)
            {
                await Shell.Current.GoToAsync("///GetStarted");
            }
            else
            {
                string? oauthToken = await SecureStorage.Default.GetAsync(AppConstants.OauthToken);
                if (oauthToken == null)
                {
                    await Shell.Current.GoToAsync("///LoginPage");
                }
                else
                {
                    await Shell.Current.GoToAsync("///HomeTab/HomePage");
                }
            }
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using DMF.Constants;

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
                // Check the correct token key that AuthService actually writes
                string? authToken = await SecureStorage.Default.GetAsync(AppKeys.AuthToken);
                if (string.IsNullOrEmpty(authToken))
                {
                    await Shell.Current.GoToAsync("///login");
                }
                else
                {
                    await Shell.Current.GoToAsync("///mainPage");
                }
            }
        }
    }
}

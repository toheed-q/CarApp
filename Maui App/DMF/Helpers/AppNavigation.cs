using DMF.Constants;

namespace DMF.Helpers
{
    // The app's start-up destination logic, shared by the Splash screen and the
    // "Later" button on the optional-update screen so both route identically.
    public static class AppNavigation
    {
        public static async Task GoToStartDestinationAsync()
        {
            string? getStarted = await SecureStorage.Default.GetAsync(AppConstants.GetStarted);
            if (getStarted == null)
            {
                await Shell.Current.GoToAsync("///GetStarted");
                return;
            }

            string? authToken = await SecureStorage.Default.GetAsync(AppKeys.AuthToken);
            await Shell.Current.GoToAsync(
                string.IsNullOrEmpty(authToken) ? "///login" : "///mainPage");
        }
    }
}

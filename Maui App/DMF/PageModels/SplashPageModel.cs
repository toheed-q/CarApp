using CommunityToolkit.Mvvm.ComponentModel;
using DMF.Helpers;

namespace DMF.PageModels
{
    public partial class SplashPageModel : ObservableObject
    {
        public SplashPageModel()
        {
        }

        public async void NavigateToHomePage()
        {
            // Force/soft update gate: if the hosted version.json requires (or offers)
            // an update, route to the update screen instead of into the app.
            var update = await AppUpdateHelper.CheckAsync();
            if (update != AppUpdateHelper.Result.UpToDate)
            {
                await Shell.Current.GoToAsync("///UpdateRequired");
                return;
            }

            await AppNavigation.GoToStartDestinationAsync();
        }
    }
}

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using DMF.Helpers;

namespace DMF
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    // Deep link: dmfmotors://car/{id}. The Netlify landing page redirects here when the
    // app is installed, so a shared car opens straight on its detail page.
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "dmfmotors",
        DataHost = "car")]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Cold start via a link: queue the id now; Home consumes it once the app
            // is past splash/login (the Shell isn't built yet at this point).
            HandleDeepLink(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            // Warm start: the app is already running, so open the car immediately.
            HandleDeepLink(intent);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Deliver the system photo-picker (multi-select) result back to
            // PhotoPickerService. This works now that the source choice is a native
            // action sheet — the old CommunityToolkit popup was killing the result.
            if (requestCode == PhotoPickerService.RequestCode)
                PhotoPickerService.DeliverResult(this, resultCode, data);
        }

        private static void HandleDeepLink(Intent? intent)
        {
            var data = intent?.Data;
            if (data == null ||
                !string.Equals(data.Scheme, "dmfmotors", System.StringComparison.OrdinalIgnoreCase))
                return;

            // dmfmotors://car/21  ->  LastPathSegment == "21"
            if (int.TryParse(data.LastPathSegment, out var id) && id > 0)
            {
                DeepLinkHandler.PendingCarId = id;

                // If the Shell is already up (warm app), open it now on the UI thread;
                // otherwise ConsumePendingAsync no-ops and Home picks it up on load.
                MainThread.BeginInvokeOnMainThread(async () =>
                    await DeepLinkHandler.ConsumePendingAsync());
            }
        }
    }
}

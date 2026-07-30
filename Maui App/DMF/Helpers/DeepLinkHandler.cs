using DMF.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DMF.Helpers
{
    /// <summary>
    /// Bridges an incoming deep link (dmfmotors://car/{id}) to in-app navigation.
    ///
    /// The platform layer (Android MainActivity) parses the URL and stores the car id
    /// here. The id is then consumed either immediately — if the app is already running
    /// and the Shell is ready — or on the next Home appearance for a cold start, where
    /// the Shell/login flow isn't built yet when the intent arrives.
    /// </summary>
    public static class DeepLinkHandler
    {
        // Set by the platform layer; cleared once opened. int? so "nothing pending"
        // is distinct from a real id.
        public static int? PendingCarId { get; set; }

        /// <summary>
        /// Opens the queued car (if any). Safe to call repeatedly and from any thread's
        /// dispatch — it no-ops when there's nothing pending or the Shell isn't ready
        /// yet, and never throws (a stale/broken link must not crash the app).
        /// </summary>
        public static async Task ConsumePendingAsync()
        {
            var id = PendingCarId;
            if (id is null || id <= 0)
                return;

            // Before login the Shell has no navigation stack to push onto; leave the id
            // queued so Home can consume it once the user is in.
            if (Shell.Current is null)
                return;

            var carService = IPlatformApplication.Current?.Services?.GetService<ICarService>();
            if (carService is null)
                return;

            // Consume up-front so a slow fetch can't double-open on a second trigger.
            PendingCarId = null;

            try
            {
                var car = await carService.GetCarForShareAsync(id.Value);
                if (car is null)
                    return;

                await Shell.Current.GoToAsync("cardetails", new Dictionary<string, object>
                {
                    { "carDetail", car }
                });
            }
            catch
            {
                // Broken/expired link, offline, etc. — silently ignore; the user just
                // lands wherever they already were.
            }
        }
    }
}

using System.Text.Json;
using DMF.Models;

namespace DMF.Helpers
{
    // Force-update check. On startup the app fetches version.json (hosted on the
    // web — Netlify for now, derived from ApiConstants.ShareBaseUrl so it moves in
    // one place when the site gets a real domain) and compares the current build's
    // Android versionCode against MinVersion.
    //
    // Fail-open: any network/parse error returns UpToDate so a flaky connection can
    // never lock a user out of the app.
    public static class AppUpdateHelper
    {
        public const string PlayStoreMarket = "market://details?id=com.dmf.services";
        public const string PlayStoreWeb    = "https://play.google.com/store/apps/details?id=com.dmf.services";

        public enum Result { UpToDate, Optional, Forced }

        // Details for the update screen, set by the last CheckAsync call.
        public static bool IsForced { get; private set; }
        public static string Message { get; private set; } = string.Empty;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<Result> CheckAsync()
        {
            try
            {
                int current = GetCurrentVersionCode();
                if (current <= 0) return Result.UpToDate;

                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(6) };
                // Cache-bust so a freshly deployed version.json is picked up immediately.
                var url = $"{ApiConstants.ShareBaseUrl}version.json?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                var json = await http.GetStringAsync(url);

                var info = JsonSerializer.Deserialize<AppVersionInfo>(json, JsonOpts);
                if (info == null) return Result.UpToDate;

                Message = info.Message ?? string.Empty;

                if (current < info.MinVersion)
                {
                    IsForced = true;
                    return Result.Forced;
                }
                if (current < info.LatestVersion)
                {
                    IsForced = false;
                    return Result.Optional;
                }
                return Result.UpToDate;
            }
            catch
            {
                // Never block the user because the check failed.
                return Result.UpToDate;
            }
        }

        // Opens the Play Store on this app's page (native store app first, web fallback).
        public static async Task OpenStoreAsync()
        {
            try { await Launcher.Default.OpenAsync(PlayStoreMarket); }
            catch
            {
                try { await Launcher.Default.OpenAsync(PlayStoreWeb); } catch { /* ignore */ }
            }
        }

        // Android versionCode is exposed as AppInfo.BuildString.
        private static int GetCurrentVersionCode() =>
            int.TryParse(AppInfo.Current.BuildString, out var v) ? v : 0;
    }
}

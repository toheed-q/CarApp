namespace DMF.Models
{
    // Shape of version.json hosted on the web (Netlify for now). Lets us require
    // an update without shipping a new build — just edit the JSON + redeploy.
    public class AppVersionInfo
    {
        // Below this Android versionCode the app MUST update (hard/force update).
        public int MinVersion { get; set; }

        // Newest versionCode available (used for a soft "update available" prompt).
        public int LatestVersion { get; set; }

        // Optional message shown on the update screen.
        public string? Message { get; set; }
    }
}

namespace DMF
{
    public static class ApiConstants
    {
        // ===== PRODUCTION (Azure) — active for the release AAB =====
        public static string BaseUrl =>
            "https://dmf-api-bwf2hkbsdaa0b3fv.centralindia-01.azurewebsites.net/api/1.0/";

        // ===== LOCAL DEV — restore this block to test against the API on this PC =====
        // Android emulator reaches the host machine via 10.0.2.2 (port 5400).
        //public static string BaseUrl =>
        //#if ANDROID
        //        "http://10.0.2.2:5400/api/1.0/";
        //#else
        //        "https://localhost:5401/api/1.0/";
        //#endif

        // Public web landing page (hosted on Netlify) that powers car share links.
        // Opening <ShareBaseUrl>?id=21 either deep-links into the app (if installed)
        // or redirects to the Play Store. Must end with a trailing slash.
        public static string ShareBaseUrl =>
            "https://luminous-boba-ecb3c9.netlify.app/";
    }
}

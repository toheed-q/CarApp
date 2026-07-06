namespace DMF
{
    public static class ApiConstants
    {
        // Live API on Azure App Service — the app now works from any device,
        // not just the emulator on the dev machine.
        public static string BaseUrl =>
            "https://dmf-api-bwf2hkbsdaa0b3fv.centralindia-01.azurewebsites.net/api/1.0/";

        // For local development, comment the line above and uncomment this block
        // (Android emulator reaches the host machine via 10.0.2.2):
        //public static string BaseUrl =>
        //#if ANDROID
        //        "http://10.0.2.2:5400/api/1.0/";
        //#else
        //        "https://localhost:5401/api/1.0/";
        //#endif
    }
}

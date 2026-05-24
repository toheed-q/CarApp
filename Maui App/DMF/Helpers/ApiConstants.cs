namespace DMF
{
    public static class ApiConstants
    {
        public static string BaseUrl =>
#if ANDROID
        "http://10.0.2.2:5098/api/1.0/";
#elif IOS
        "https://localhost:7049/api/1.0/";
#else
        "https://localhost:7049/api/1.0/";
#endif
    }
}

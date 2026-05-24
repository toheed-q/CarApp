using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using PopupService = DMF.Services.PopupService;


#if ANDROID
using Android.Views;
#endif

namespace DMF
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
#if IOS || MACCATALYST
                    handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
#endif
#if ANDROID
                    Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                    {
                        handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                        handler.PlatformView.BackgroundTintList = null;
                        System.Diagnostics.Debug.WriteLine("Android Entry background removed");
                    });

                    Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                    {
                        handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                        handler.PlatformView.BackgroundTintList = null;
                        System.Diagnostics.Debug.WriteLine("Android Picker background removed");
                    });
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("Poppins-SemiBold.ttf", "PoppinsSemiBold");
                    fonts.AddFont("Poppins-Regular.ttf", "PoppinsRegular");
                    fonts.AddFont("Poppins-Medium.ttf", "PoppinsMedium");
                    fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });


#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif
            builder.Services.AddHttpClient<IApiService, ApiService>(client =>
            {
                client.BaseAddress = new Uri(ApiConstants.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            builder.Services.AddSingleton<IBlobService>(sp => new BlobService("YOUR_CONNECTION_STRING", "cars"
    ));

            builder.Services.AddSingleton<AppTabsPage>();
            builder.Services.AddSingleton<ProjectRepository>();
            builder.Services.AddSingleton<TaskRepository>();
            builder.Services.AddSingleton<CategoryRepository>();
            builder.Services.AddSingleton<TagRepository>();
            builder.Services.AddSingleton<SeedDataService>();
            builder.Services.AddSingleton<ModalErrorHandler>();
            builder.Services.AddSingleton<SplashPageModel>();
            builder.Services.AddSingleton<GetStartedPageModel>();
            builder.Services.AddSingleton<ProjectListPageModel>();

            builder.Services.AddSingleton<MainPage, MainPageModel>();

            builder.Services.AddTransient<HomeView, HomeViewModel>();
            builder.Services.AddTransient<LoginPage, LoginPageModel>();
            builder.Services.AddTransient<AccountView, AccountViewModel>();
            builder.Services.AddTransient<FavoriteView, FavoriteViewModel>();

            builder.Services.AddTransient<ICarService, CarService>();
            builder.Services.AddTransient<IAuthService, AuthService>();
            builder.Services.AddTransient<IPopupService, PopupService>();
            builder.Services.AddTransient<IUserDetailService, UserDetailService>();
            builder.Services.AddTransient<ISecureStorageService, SecureStorageService>();


            builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");
            builder.Services.AddTransientWithShellRoute<WishlistPage, WishlistPageModel>("wishlist");
            builder.Services.AddTransientWithShellRoute<ContactUsPage, ContactUsPageModel>("contactus");
            builder.Services.AddTransientWithShellRoute<CarDetailPage, CarDetailPageModel>("cardetails");
            builder.Services.AddTransientWithShellRoute<ProfileViewPage, ProfileViewPageModel>("profile");
            builder.Services.AddTransientWithShellRoute<AddCarStep1Page, AddCarViewModel>("AddCarStep1");
            builder.Services.AddTransientWithShellRoute<AddCarStep2Page, AddCarViewModel>("AddCarStep2");
            builder.Services.AddTransientWithShellRoute<AddCarStep3Page, AddCarViewModel>("AddCarStep3");
            builder.Services.AddTransientWithShellRoute<AddImageUploadPage, AddCarViewModel>("AddCarStep4");
            builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
            builder.Services.AddTransientWithShellRoute<OTPVerificationPage, OTPVerificationPageModel>("otpverification");

            return builder.Build();
        }
    }
}

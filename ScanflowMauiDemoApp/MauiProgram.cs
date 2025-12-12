using CommunityToolkit.Maui;
using Mopups.Hosting;
using ScanflowMauiDemoApp.Services;
using ScanflowMauiDemoApp.Views;
#if ANDROID
using Scanflow.BarcodeCapture.Maui;
#elif IOS
using ScanflowMauiDemoApp.Platforms.iOS;
#endif

namespace ScanflowMauiDemoApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureMopups()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseMauiCommunityToolkit()
                .ConfigureMauiHandlers(handler =>
                {
#if ANDROID
                    InitializeScanflow.UseScanflow(handler);
#elif IOS
                    handler.AddHandler<IOSCameraView, IOSCameraViewHandler>();
#endif
                });
            
            // Register Services (Singleton for shared state across app)
            builder.Services.AddSingleton<IScanflowService, ScanflowService>();
            
            // Register Pages
            builder.Services.AddTransient<SplashPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<ScanViewPage>();

            return builder.Build();
        }
    }
}

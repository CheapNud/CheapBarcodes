using CheapHelpers.Services;
using Microsoft.Extensions.Logging;

namespace CheapBarcodes
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddSingleton<HttpClient>(serviceProvider =>
            {
                var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(2)
                };
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                return httpClient;
            });

            builder.Services.AddMauiBlazorWebView();

            //add service dependencies
            builder.Services.AddSingleton<IBarcodeService, BarcodeService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

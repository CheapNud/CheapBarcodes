using CheapHelpers.Services;
using CheapHelpers.Services.Communication.Barcode;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;

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

            // Add HTTP client
            builder.Services.AddSingleton<HttpClient>(serviceProvider =>
            {
                var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(2)
                };
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                return httpClient;
            });

            // Add MAUI Blazor WebView
            builder.Services.AddMauiBlazorWebView();

            // Add MudBlazor services
            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 3000;
                config.SnackbarConfiguration.HideTransitionDuration = 250;
                config.SnackbarConfiguration.ShowTransitionDuration = 250;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

            // Add barcode service dependencies
            builder.Services.AddSingleton<IBarcodeService, BarcodeService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
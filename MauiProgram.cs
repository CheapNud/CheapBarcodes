using CheapBarcodes.Scanning;
using CheapBarcodes.Services;
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

            // Hardware scanner (CheapBarcodes.Scanning): real implementation on Android, no-op elsewhere
#if ANDROID
            builder.Services.AddSingleton<IHardwareScannerService, AndroidHardwareScannerService>();
#else
            builder.Services.AddSingleton<IHardwareScannerService, NullHardwareScannerService>();
#endif

            // Keyboard-wedge (USB/Bluetooth HID) scanners feed the same scan pipeline
            builder.Services.AddSingleton(serviceProvider =>
            {
                var wedgeDetector = new KeyboardWedgeDetector();
                wedgeDetector.BarcodeScanned += barcode =>
                    serviceProvider.GetService<IHardwareScannerService>()?.OnScan(barcode);
                return wedgeDetector;
            });

            // Configurable scan phone-home (see ApiSettings page)
            builder.Services.AddSingleton<ScanApiClient>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
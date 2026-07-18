using CheapBarcodes.Scanning;
using CheapBarcodes.Services;
using CheapHelpers.Services;
using CheapHelpers.Services.Communication.Barcode;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using Serilog;

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

            // Structured logging: rolling local file always, Seq when configured in settings
            var loggingOptions = LoggingOptions.Load();
            var serilogConfig = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.File(Path.Combine(LoggingOptions.LogDirectory, "cheapbarcodes-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7);

            if (loggingOptions.IsSeqConfigured)
            {
                serilogConfig = serilogConfig.WriteTo.Seq(loggingOptions.SeqUrl,
                    apiKey: string.IsNullOrWhiteSpace(loggingOptions.SeqApiKey) ? null : loggingOptions.SeqApiKey);
            }

            builder.Logging.AddSerilog(serilogConfig.CreateLogger(), dispose: true);

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
                    serviceProvider.GetService<IHardwareScannerService>()?.OnScan(new ScanResult(barcode, ScanSource.KeyboardWedge));
                return wedgeDetector;
            });

            // Configurable scan phone-home (see ApiSettings page) with offline retry queue
            builder.Services.AddSingleton<ScanApiClient>();
            builder.Services.AddSingleton<ScanUploadQueue>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
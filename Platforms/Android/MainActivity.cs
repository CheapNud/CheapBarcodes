using Android.App;
using Android.Content.PM;
using Android.OS;
using CheapBarcodes.Scanning;

namespace CheapBarcodes.Platforms.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
        ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private Rt150ScannerHost? _scannerHost;
        private IHardwareScannerService? _hardwareScannerService;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _scannerHost = new Rt150ScannerHost(this);
            _scannerHost.BarcodeScanned += OnBarcodeScanned;
        }

        protected override void OnStart()
        {
            base.OnStart();
            _scannerHost?.Start();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _scannerHost?.Start();
        }

        protected override void OnPause()
        {
            base.OnPause();
            _scannerHost?.Stop();
        }

        protected override void OnDestroy()
        {
            _scannerHost?.Dispose();
            _hardwareScannerService?.StopScanning();
            base.OnDestroy();
        }

        private void OnBarcodeScanned(string barcode)
        {
            // Resolve lazily - MAUI DI is guaranteed up by the time a scan arrives
            if (_hardwareScannerService == null)
            {
                _hardwareScannerService = IPlatformApplication.Current?.Services?.GetService<IHardwareScannerService>();
                _hardwareScannerService?.StartScanning();
            }

            _hardwareScannerService?.OnScan(barcode);
        }
    }
}

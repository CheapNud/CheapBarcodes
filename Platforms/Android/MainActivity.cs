using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using CheapBarcodes;
using CheapBarcodes.Platforms.Android;
using CheapHelpers.Services;
using static MudBlazor.Defaults;

namespace MecamApplication.Handheld
{
    // [IntentFilter(new[] { "com.android.serial.BARCODEPORT_RECEIVEDDATA_ACTION" })]
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private BroadcastReceiver _receiver;
        private static Handler _handler;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _handler = new Handler(OnScan);

            try
            {
                System.Diagnostics.Debug.WriteLine("Main: OnCreate");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("OnCreate exception: " + ex.Message);
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            try
            {
                System.Diagnostics.Debug.WriteLine("ScanView: OnStart");

                IntentFilter filter = new IntentFilter();
                filter.AddAction("com.android.serial.BARCODEPORT_RECEIVEDDATA_ACTION");
                filter.AddAction("android.rfid.FUN_KEY");
                _receiver = new BarcodeReceiver(_handler);
                RegisterReceiver(_receiver, filter);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ScanView OnStart exception: " + ex.Message);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ScanView: OnDestroy");
                _receiver?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ScanView OnDestroy exception:" + ex.Message);
            }
            base.OnDestroy();
        }

        protected override void OnPause()
        {
            base.OnPause();
            try
            {
                System.Diagnostics.Debug.WriteLine("ScanView: OnPause");

                if (_receiver != null)
                {
                    UnregisterReceiver(_receiver);
                }
                _receiver?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ScanBase OnPause exception: " + ex.Message);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            try
            {
                System.Diagnostics.Debug.WriteLine("ScanView: OnResume");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ScanView OnResume exception:" + ex.Message);
            }
        }

        public virtual void OnScan(Message m)
        {
            try
            {
                // Ensure IPlatformApplication.Current is not null
                if (IPlatformApplication.Current == null)
                {
                    System.Diagnostics.Debug.WriteLine("IPlatformApplication.Current is null.");
                    return;
                }

                // Ensure Services is not null
                var serviceProvider = IPlatformApplication.Current.Services;
                if (serviceProvider == null)
                {
                    System.Diagnostics.Debug.WriteLine("ServiceProvider is null.");
                    return;
                }

                // Resolve IBarcodeService safely
                var bs = serviceProvider.GetService<IBarcodeService>();
                if (bs == null)
                {
                    System.Diagnostics.Debug.WriteLine("IBarcodeService is not registered.");
                    return;
                }

                bs.OnScan(m.Data.GetString("data"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ScanView OnScan: " + ex.Message);
                throw;
            }
        }
    }
}

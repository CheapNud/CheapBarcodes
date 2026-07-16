using Android.App;
using Android.Content;
using Android.OS;

namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// Generic broadcast-intent scanner host for the many Android handhelds that
    /// deliver scans as a broadcast with a string extra - Zebra DataWedge,
    /// Honeywell, and most budget vendors. Configure the action and extra key to
    /// match the device (e.g. DataWedge profile action + "com.symbol.datawedge.data_string").
    /// Wire into the activity lifecycle like Rt150ScannerHost: Start in
    /// OnStart/OnResume, Stop in OnPause, Dispose in OnDestroy.
    /// </summary>
    public class IntentScannerHost(Activity activity, string barcodeAction, string barcodeExtraKey) : IDisposable
    {
        private BroadcastReceiver? _receiver;

        public event Action<ScanResult>? ScanReceived;

        public bool IsStarted { get; private set; }

        public void Start()
        {
            if (IsStarted)
            {
                return;
            }

            try
            {
                _receiver = new IntentScanReceiver(this);
                var filter = new IntentFilter();
                filter.AddAction(barcodeAction);

                // API 34+ throws without an export flag on non-system broadcasts; the
                // flags overload exists since API 26. Vendor firmware broadcasts come
                // from another process, so they must be exported
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    activity.RegisterReceiver(_receiver, filter, ReceiverFlags.Exported);
                }
                else
                {
                    activity.RegisterReceiver(_receiver, filter);
                }

                IsStarted = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting IntentScannerHost: {ex.Message}");
            }
        }

        public void Stop()
        {
            try
            {
                if (_receiver != null)
                {
                    activity.UnregisterReceiver(_receiver);
                    _receiver.Dispose();
                    _receiver = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping IntentScannerHost: {ex.Message}");
            }
            finally
            {
                IsStarted = false;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void HandleIntent(Intent? intent)
        {
            var barcode = intent?.GetStringExtra(barcodeExtraKey);
            if (string.IsNullOrEmpty(barcode))
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"IntentScannerHost received barcode: {barcode}");
            ScanReceived?.Invoke(new ScanResult(barcode, ScanSource.Broadcast));
        }

        private class IntentScanReceiver : BroadcastReceiver
        {
            private readonly IntentScannerHost _host;

            public IntentScanReceiver(IntentScannerHost host)
            {
                _host = host;
            }

            public override void OnReceive(Context? context, Intent? intent)
            {
                _host.HandleIntent(intent);
            }
        }
    }
}

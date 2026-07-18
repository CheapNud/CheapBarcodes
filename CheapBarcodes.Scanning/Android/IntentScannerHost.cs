using Android.Content;
using Android.OS;
using Microsoft.Extensions.Logging;

namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// Generic broadcast-intent scanner host for the many Android handhelds that
    /// deliver scans as a broadcast - Zebra DataWedge, Honeywell, Urovo, and most
    /// budget vendors. Register one or more <see cref="IntentScannerProfile"/>s;
    /// whichever device's broadcast fires produces the same ScanResult stream.
    /// Any Context works (Activity, Application, foreground Service). Wire into
    /// the host lifecycle: Start in OnStart/OnResume, Stop in OnPause, Dispose in
    /// OnDestroy.
    /// </summary>
    public class IntentScannerHost(Context context, params IntentScannerProfile[] profiles) : IDisposable
    {
        private BroadcastReceiver? _receiver;

        /// <summary>
        /// Single-device convenience: one action, one string extra.
        /// </summary>
        public IntentScannerHost(Context context, string barcodeAction, string barcodeExtraKey)
            : this(context, new IntentScannerProfile { Actions = [barcodeAction], DataExtraKeys = [barcodeExtraKey] })
        {
        }

        public event Action<ScanResult>? ScanReceived;

        /// <summary>Optional logger; without one the host is silent.</summary>
        public ILogger? Logger { get; init; }

        public bool IsStarted { get; private set; }

        public void Start()
        {
            if (IsStarted || profiles.Length == 0)
            {
                return;
            }

            try
            {
                _receiver = new IntentScanReceiver(this);
                var filter = new IntentFilter();
                foreach (var action in profiles.SelectMany(scannerProfile => scannerProfile.Actions).Distinct())
                {
                    filter.AddAction(action);
                }

                // API 34+ throws without an export flag on non-system broadcasts; the
                // flags overload exists since API 26. Vendor firmware broadcasts come
                // from another process, so they must be exported
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    context.RegisterReceiver(_receiver, filter, ReceiverFlags.Exported);
                }
                else
                {
                    context.RegisterReceiver(_receiver, filter);
                }

                IsStarted = true;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error starting IntentScannerHost");
            }
        }

        public void Stop()
        {
            try
            {
                if (_receiver != null)
                {
                    context.UnregisterReceiver(_receiver);
                    _receiver.Dispose();
                    _receiver = null;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error stopping IntentScannerHost");
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
            if (intent?.Action == null)
            {
                return;
            }

            foreach (var scannerProfile in profiles)
            {
                if (!scannerProfile.Actions.Contains(intent.Action))
                {
                    continue;
                }

                var barcode = ExtractBarcode(intent, scannerProfile);
                if (string.IsNullOrEmpty(barcode))
                {
                    continue;
                }

                string? barcodeFormat = scannerProfile.FormatExtraKey != null
                    ? intent.GetStringExtra(scannerProfile.FormatExtraKey)
                    : null;

                Logger?.LogDebug("IntentScannerHost received barcode ({Action}): {Barcode}", intent.Action, barcode);
                ScanReceived?.Invoke(new ScanResult(barcode, ScanSource.Broadcast, barcodeFormat));
                return;
            }
        }

        private static string? ExtractBarcode(Intent intent, IntentScannerProfile scannerProfile)
        {
            foreach (var extraKey in scannerProfile.DataExtraKeys)
            {
                var stringValue = intent.GetStringExtra(extraKey);
                if (!string.IsNullOrEmpty(stringValue))
                {
                    return stringValue;
                }
            }

            foreach (var extraKey in scannerProfile.ByteArrayExtraKeys)
            {
                var byteValue = intent.GetByteArrayExtra(extraKey);
                if (byteValue is { Length: > 0 })
                {
                    int length = scannerProfile.LengthExtraKey != null
                        ? Math.Clamp(intent.GetIntExtra(scannerProfile.LengthExtraKey, byteValue.Length), 0, byteValue.Length)
                        : byteValue.Length;

                    if (length > 0)
                    {
                        return scannerProfile.DataEncoding.GetString(byteValue, 0, length);
                    }
                }
            }

            return null;
        }

        private class IntentScanReceiver : BroadcastReceiver
        {
            private readonly IntentScannerHost _host;

            public IntentScanReceiver(IntentScannerHost host)
            {
                _host = host;
            }

            public override void OnReceive(Context? receiverContext, Intent? intent)
            {
                _host.HandleIntent(intent);
            }
        }
    }
}

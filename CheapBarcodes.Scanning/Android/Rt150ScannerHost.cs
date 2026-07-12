using Android.App;
using Android.Content;
using Android.OS;
using CN.Pda.Scan;
using CN.Pda.Serialport;

namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// Hosts the RT150 scanner plumbing (serial-port scan thread, hardware key and
    /// barcode broadcast receivers) for an Activity. Wire it into the activity
    /// lifecycle: construct in OnCreate, Start in OnStart/OnResume, Stop in OnPause,
    /// Dispose in OnDestroy. Subscribe to <see cref="BarcodeScanned"/> for raw scans.
    /// </summary>
    public class Rt150ScannerHost : IDisposable
    {
        private const string BarcodeAction = "com.android.serial.BARCODEPORT_RECEIVEDDATA_ACTION";
        private const string FunctionKeyAction = "android.rfid.FUN_KEY";

        private readonly Activity _activity;
        private readonly Handler _handler;
        private ScanThread? _scanThread;
        private BroadcastReceiver? _barcodeReceiver;
        private BroadcastReceiver? _keyReceiver;
        private bool _isStarted;

        public event Action<string>? BarcodeScanned;

        public Rt150ScannerHost(Activity activity)
        {
            _activity = activity;
            _handler = new Handler(Looper.MainLooper!, new ScanHandlerCallback(this));
        }

        public bool IsStarted => _isStarted;

        /// <summary>
        /// Initializes serial-port scanning (with broadcast fallback) and registers receivers.
        /// Safe to call repeatedly; no-ops while already started.
        /// </summary>
        public void Start()
        {
            if (_isStarted)
            {
                return;
            }

            try
            {
                if (TryInitializeSerialPortScanning())
                {
                    System.Diagnostics.Debug.WriteLine("SerialPort scanning initialized successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("SerialPort failed, using BroadcastReceiver");
                }

                // Always register broadcast receivers as fallback/additional support
                RegisterBroadcastReceivers();
                _isStarted = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting Rt150ScannerHost: {ex.Message}");
            }
        }

        /// <summary>
        /// Unregisters receivers. The scan thread keeps its serial port until Dispose.
        /// </summary>
        public void Stop()
        {
            try
            {
                if (_barcodeReceiver != null)
                {
                    _activity.UnregisterReceiver(_barcodeReceiver);
                    _barcodeReceiver.Dispose();
                    _barcodeReceiver = null;
                }

                if (_keyReceiver != null)
                {
                    _activity.UnregisterReceiver(_keyReceiver);
                    _keyReceiver.Dispose();
                    _keyReceiver = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping Rt150ScannerHost: {ex.Message}");
            }
            finally
            {
                _isStarted = false;
            }
        }

        public void Dispose()
        {
            Stop();

            try
            {
                _scanThread?.Interrupt();
                _scanThread?.Close();
                _scanThread?.Dispose();
                _scanThread = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing Rt150ScannerHost: {ex.Message}");
            }
        }

        private bool TryInitializeSerialPortScanning()
        {
            try
            {
                // Configure SerialPort settings for RT150 devices
                ScanThread.BaudRate = SerialPort.Baudrate9600;
                ScanThread.Port = SerialPort.Com0;
                ScanThread.Power = SerialPort.PowerScaner;

                // The scan thread survives Stop() and a Java thread can only be
                // started once - create and start it on the first Start() only
                if (_scanThread == null)
                {
                    _scanThread = new ScanThread(_handler);
                    _scanThread.Start();
                }

                // Receivers are re-created on every lifecycle cycle
                _keyReceiver = new KeyReceiver(_scanThread);
                RegisterVendorReceiver(_keyReceiver, FunctionKeyAction);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SerialPort initialization failed: {ex.Message}");
                return false;
            }
        }

        private void RegisterBroadcastReceivers()
        {
            try
            {
                _barcodeReceiver = new BarcodeReceiver(_handler);
                RegisterVendorReceiver(_barcodeReceiver, BarcodeAction);

                // If we don't have SerialPort scanning, also register key receiver
                if (_keyReceiver == null)
                {
                    _keyReceiver = new KeyReceiver(null); // No scan thread available
                    RegisterVendorReceiver(_keyReceiver, FunctionKeyAction);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering broadcast receivers: {ex.Message}");
            }
        }

        private void RegisterVendorReceiver(BroadcastReceiver receiver, string action)
        {
            var filter = new IntentFilter();
            filter.AddAction(action);

            // API 34+ throws without an export flag on non-system broadcasts; the
            // flags overload exists since API 26, so apply it broadly. These come
            // from vendor scanner firmware (another process), so they must be exported
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                _activity.RegisterReceiver(receiver, filter, ReceiverFlags.Exported);
            }
            else
            {
                _activity.RegisterReceiver(receiver, filter);
            }
        }

        private void HandleScanMessage(Message message)
        {
            try
            {
                string? barcode = message.Data?.GetString("data");
                if (string.IsNullOrEmpty(barcode))
                {
                    System.Diagnostics.Debug.WriteLine("Received empty barcode message");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Rt150ScannerHost received barcode: {barcode}");
                BarcodeScanned?.Invoke(barcode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing scan message: {ex.Message}");
            }
        }

        private class ScanHandlerCallback : Java.Lang.Object, Handler.ICallback
        {
            private readonly Rt150ScannerHost _host;

            public ScanHandlerCallback(Rt150ScannerHost host)
            {
                _host = host;
            }

            public bool HandleMessage(Message msg)
            {
                _host.HandleScanMessage(msg);
                return true;
            }
        }
    }
}

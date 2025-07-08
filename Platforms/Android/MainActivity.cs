using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using CheapBarcodes.Platforms.Android;
using CheapBarcodes.Services;
using CN.Pda.Scan;
using CN.Pda.Serialport;

namespace CheapBarcodes.Platforms.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
        ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private BroadcastReceiver _barcodeReceiver;
        private BroadcastReceiver _keyReceiver;
        private static Handler _handler;
        private static ScanThread _scanThread;
        private IHardwareScannerService _hardwareScannerService;
        private bool _isScanning = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            try
            {
                System.Diagnostics.Debug.WriteLine("MainActivity: OnCreate");

                // Use modern Handler constructor with Looper
                _handler = new Handler(Looper.MainLooper, new HandlerCallback(this));

                // Get the hardware scanner service - will be available after MAUI initialization
                Task.Run(async () =>
                {
                    // Wait for MAUI to initialize
                    await Task.Delay(100);
                    _hardwareScannerService = IPlatformApplication.Current?.Services?.GetService<IHardwareScannerService>();
                    if (_hardwareScannerService != null)
                    {
                        _hardwareScannerService.StartScanning();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainActivity OnCreate exception: {ex.Message}");
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            try
            {
                System.Diagnostics.Debug.WriteLine("MainActivity: OnStart");
                RegisterScanningReceivers();
                _isScanning = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainActivity OnStart exception: {ex.Message}");
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            try
            {
                System.Diagnostics.Debug.WriteLine("MainActivity: OnResume");

                // Ensure scanning is active
                if (!_isScanning)
                {
                    RegisterScanningReceivers();
                    _isScanning = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainActivity OnResume exception: {ex.Message}");
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            try
            {
                System.Diagnostics.Debug.WriteLine("MainActivity: OnPause");
                UnregisterScanningReceivers();
                _isScanning = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainActivity OnPause exception: {ex.Message}");
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainActivity: OnDestroy");

                UnregisterScanningReceivers();
                _scanThread?.Interrupt();
                _scanThread?.Close();
                _scanThread?.Dispose();
                _hardwareScannerService?.StopScanning();
                _isScanning = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainActivity OnDestroy exception: {ex.Message}");
            }
            finally
            {
                base.OnDestroy();
            }
        }

        private void RegisterScanningReceivers()
        {
            try
            {
                // First, try to initialize SerialPort scanning (for RT150 devices)
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering scanning receivers: {ex.Message}");
            }
        }

        private bool TryInitializeSerialPortScanning()
        {
            try
            {
                // Configure SerialPort settings
                ScanThread.BaudRate = SerialPort.Baudrate9600;
                ScanThread.Port = SerialPort.Com0;
                ScanThread.Power = SerialPort.PowerScaner;

                // Create and start scan thread
                _scanThread = new ScanThread(_handler);

                // Register key receiver for hardware buttons
                _keyReceiver = new KeyReceiver(_scanThread);
                IntentFilter keyFilter = new IntentFilter();
                keyFilter.AddAction("android.rfid.FUN_KEY");
                RegisterReceiver(_keyReceiver, keyFilter);

                _scanThread.Start();
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
                // Register barcode data receiver
                IntentFilter barcodeFilter = new IntentFilter();
                barcodeFilter.AddAction("com.android.serial.BARCODEPORT_RECEIVEDDATA_ACTION");

                _barcodeReceiver = new BarcodeReceiver(_handler);
                RegisterReceiver(_barcodeReceiver, barcodeFilter);

                // If we don't have SerialPort scanning, also register key receiver
                if (_keyReceiver == null)
                {
                    _keyReceiver = new KeyReceiver(null); // No scan thread available
                    IntentFilter keyFilter = new IntentFilter();
                    keyFilter.AddAction("android.rfid.FUN_KEY");
                    RegisterReceiver(_keyReceiver, keyFilter);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering broadcast receivers: {ex.Message}");
            }
        }

        private void UnregisterScanningReceivers()
        {
            try
            {
                if (_barcodeReceiver != null)
                {
                    UnregisterReceiver(_barcodeReceiver);
                    _barcodeReceiver.Dispose();
                    _barcodeReceiver = null;
                }

                if (_keyReceiver != null)
                {
                    UnregisterReceiver(_keyReceiver);
                    _keyReceiver.Dispose();
                    _keyReceiver = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error unregistering receivers: {ex.Message}");
            }
        }

        public virtual void OnScanMessage(Message message)
        {
            try
            {
                if (message?.Data == null)
                {
                    System.Diagnostics.Debug.WriteLine("Received null message or data");
                    return;
                }

                string barcode = message.Data.GetString("data");
                if (string.IsNullOrEmpty(barcode))
                {
                    System.Diagnostics.Debug.WriteLine("Received empty barcode");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"MainActivity received barcode: {barcode}");

                // Process through the hardware scanner service (CORRECT REFERENCE)
                _hardwareScannerService?.OnScan(barcode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing scan message: {ex.Message}");
            }
        }
    }

    // Modern Handler.Callback implementation
    internal class HandlerCallback : Java.Lang.Object, Handler.ICallback
    {
        private readonly MainActivity _activity;

        public HandlerCallback(MainActivity activity)
        {
            _activity = activity;
        }

        public bool HandleMessage(Message msg)
        {
            _activity.OnScanMessage(msg);
            return true;
        }
    }
}
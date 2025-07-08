using Android.Content;
using Android.OS;
using Android.Views;
using CN.Pda.Scan;
using CheapBarcodes.Helpers;

namespace CheapBarcodes.Platforms.Android
{
    public class KeyReceiver : BroadcastReceiver
    {
        private readonly ScanThread _scanThread;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(1.8);
        private DateTime _lastKeyTime = DateTime.MinValue;

        public KeyReceiver(ScanThread scanThread)
        {
            _scanThread = scanThread;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                if (DateTime.Now - _lastKeyTime < _timeout)
                {
                    System.Diagnostics.Debug.WriteLine("Key press ignored - within timeout period");
                    return;
                }

                if (!intent.GetBooleanExtra("keydown", false))
                {
                    return; // Only process key down events
                }

                int keyCode = intent.GetIntExtra("keyCode", 0);
                ProcessKeyCode(keyCode);

                _lastKeyTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"KeyReceiver OnReceive exception: {ex.Message}");
            }
        }

        private void ProcessKeyCode(int keyCode)
        {
            switch ((Keycode)keyCode)
            {
                case Keycode.F1:
                    System.Diagnostics.Debug.WriteLine($"{keyCode} F1 - Scan trigger");
                    TriggerScan();
                    break;

                case Keycode.F2:
                    System.Diagnostics.Debug.WriteLine($"{keyCode} F2 - Scan trigger");
                    TriggerScan();
                    break;

                case Keycode.F3:
                    System.Diagnostics.Debug.WriteLine($"{keyCode} F3 - Pistol Key");
                    TriggerScan();
                    break;

                case Keycode.F4:
                    System.Diagnostics.Debug.WriteLine($"{keyCode} F4 - Scan trigger");
                    TriggerScan();
                    break;

                case Keycode.F5:
                    System.Diagnostics.Debug.WriteLine($"{keyCode} F5 - Scan trigger");
                    TriggerScan();
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"Unhandled key code: {keyCode}");
                    break;
            }
        }

        private void TriggerScan()
        {
            try
            {
                if (_scanThread != null)
                {
                    // Use SerialPort scanning if available
                    _scanThread.Scan();
                    System.Diagnostics.Debug.WriteLine("Triggered SerialPort scan");
                }
                else
                {
                    // If no SerialPort scanning, send a broadcast to trigger other scanning methods
                    System.Diagnostics.Debug.WriteLine("No SerialPort available - hardware key pressed but no scan thread");
                    // You could potentially trigger a camera-based scan here or other alternatives
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error triggering scan: {ex.Message}");
            }
        }
    }
}
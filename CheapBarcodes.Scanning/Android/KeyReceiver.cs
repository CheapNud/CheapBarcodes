using Android.Content;
using Android.Views;
using CN.Pda.Scan;
using Microsoft.Extensions.Logging;

namespace CheapBarcodes.Scanning
{
    internal class KeyReceiver : BroadcastReceiver
    {
        private readonly ScanThread? _scanThread;
        private readonly ILogger? _logger;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(1.8);
        private DateTime _lastKeyTime = DateTime.MinValue;

        public KeyReceiver(ScanThread? scanThread, ILogger? logger = null)
        {
            _scanThread = scanThread;
            _logger = logger;
        }

        public override void OnReceive(Context? context, Intent? intent)
        {
            try
            {
                if (intent == null)
                {
                    return;
                }

                if (DateTime.Now - _lastKeyTime < _timeout)
                {
                    _logger?.LogDebug("Key press ignored - within timeout period");
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
                _logger?.LogError(ex, "KeyReceiver OnReceive failed");
            }
        }

        private void ProcessKeyCode(int keyCode)
        {
            switch ((Keycode)keyCode)
            {
                case Keycode.F1:
                case Keycode.F2:
                case Keycode.F3:
                case Keycode.F4:
                case Keycode.F5:
                    _logger?.LogDebug("Hardware key {KeyCode} - scan trigger", keyCode);
                    TriggerScan();
                    break;

                default:
                    _logger?.LogDebug("Unhandled key code: {KeyCode}", keyCode);
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
                    _logger?.LogDebug("Triggered SerialPort scan");
                }
                else
                {
                    // If no SerialPort scanning, the vendor broadcast path delivers the barcode instead
                    _logger?.LogDebug("Hardware key pressed but no scan thread available");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error triggering scan");
            }
        }
    }
}

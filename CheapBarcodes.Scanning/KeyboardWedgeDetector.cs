using System.Text;

namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// Detects barcode scans from keyboard-wedge (HID) scanners - USB or Bluetooth
    /// scanners that present themselves as keyboards. Scanners type the whole code
    /// as a fast burst followed by a terminator (usually Enter); human typing has
    /// much larger gaps between keys, so slow input never accumulates.
    /// Platform-neutral: feed it characters and terminators from any key source.
    /// </summary>
    public class KeyboardWedgeDetector
    {
        private readonly StringBuilder _burstBuffer = new();
        private long _lastKeyMilliseconds;

        // ponytail: fixed defaults tuned for common wedge scanners (~10-30ms/char);
        // both are settable because real scanners and slow Bluetooth links vary
        public TimeSpan MaxInterKeyGap { get; set; } = TimeSpan.FromMilliseconds(75);
        public int MinBarcodeLength { get; set; } = 3;

        public event Action<string>? BarcodeScanned;

        /// <summary>
        /// Feed a printable character from the key source.
        /// </summary>
        public void ProcessCharacter(char inputChar)
        {
            long nowMilliseconds = Environment.TickCount64;

            // A gap larger than a scanner burst means human typing or stale input
            if (_burstBuffer.Length > 0 && nowMilliseconds - _lastKeyMilliseconds > MaxInterKeyGap.TotalMilliseconds)
            {
                _burstBuffer.Clear();
            }

            _lastKeyMilliseconds = nowMilliseconds;
            _burstBuffer.Append(inputChar);
        }

        /// <summary>
        /// Feed the terminator key (Enter or whatever suffix the scanner is programmed
        /// to send). Fires <see cref="BarcodeScanned"/> when the buffered burst
        /// qualifies as a barcode.
        /// </summary>
        public void ProcessTerminator()
        {
            var candidate = _burstBuffer.ToString();
            _burstBuffer.Clear();

            if (candidate.Length >= MinBarcodeLength)
            {
                BarcodeScanned?.Invoke(candidate);
            }
        }
    }
}

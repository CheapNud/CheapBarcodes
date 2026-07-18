using System.Text;

namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// Describes how one scanner vendor delivers scans over broadcast intents:
    /// which actions fire, which extras carry the barcode (string and/or byte
    /// array variants), and optionally where the symbology/format lives.
    /// Register several profiles on one IntentScannerHost to build an APK that
    /// works on whichever device it lands on.
    /// </summary>
    public class IntentScannerProfile
    {
        public required string[] Actions { get; init; }

        /// <summary>String extras to try, in order.</summary>
        public string[] DataExtraKeys { get; init; } = [];

        /// <summary>
        /// Byte-array extras to try when no string extra matches - some vendors
        /// (Urovo-style) deliver the barcode as bytes plus a length extra.
        /// </summary>
        public string[] ByteArrayExtraKeys { get; init; } = [];

        /// <summary>Int extra holding the byte count for byte-array payloads.</summary>
        public string? LengthExtraKey { get; init; }

        /// <summary>String extra holding the symbology/format, when the vendor sends one.</summary>
        public string? FormatExtraKey { get; init; }

        /// <summary>
        /// Encoding for byte-array payloads. Defaults to UTF-8; Chinese-market
        /// devices often use GBK (bring your own Encoding instance - GBK needs
        /// the CodePages encoding provider on .NET).
        /// </summary>
        public Encoding DataEncoding { get; init; } = Encoding.UTF8;

        /// <summary>
        /// RT150-class devices: same vendor broadcast the serial fallback uses.
        /// </summary>
        public static IntentScannerProfile Rt150 => new()
        {
            Actions = ["com.android.serial.BARCODEPORT_RECEIVEDDATA_ACTION"],
            DataExtraKeys = ["DATA"],
        };

        /// <summary>
        /// Urovo-style devices: barcode arrives as a byte array ("barocode" is
        /// the vendor's own typo) with a separate length extra. Verify against
        /// your device's scan service settings.
        /// </summary>
        public static IntentScannerProfile Urovo => new()
        {
            Actions = ["android.intent.ACTION_DECODE_DATA"],
            DataExtraKeys = ["barcode_string"],
            ByteArrayExtraKeys = ["barocode"],
            LengthExtraKey = "length",
        };
    }
}

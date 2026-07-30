using System.Text;

namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// Describes how one scanner vendor delivers scans over broadcast intents:
    /// which actions fire, which extras carry the barcode (string and/or byte
    /// array variants), and optionally where the symbology/format lives.
    /// Register several profiles on one IntentScannerHost to build an APK that
    /// works on whichever device it lands on - or start from <see cref="AllKnown"/>.
    ///
    /// Vendors with app-defined actions (Zebra DataWedge, Honeywell) have no
    /// honest preset - construct a profile with the action you configured on the
    /// device; see the package README for the per-vendor patterns. Point Mobile
    /// delivers no data in its broadcast at all (SDK call required) and cannot be
    /// supported by a profile.
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
        /// RT150-class and generic Chinese PDA family. Officially documented by
        /// WEROCK for the Scoria series (Zebra SE4710 engine), so this convention
        /// spans a whole device family, not one model.
        /// </summary>
        public static IntentScannerProfile Rt150 => new()
        {
            Actions = ["com.android.serial.BARCODEPORT_RECEIVEDDATA_ACTION"],
            DataExtraKeys = ["DATA"],
        };

        /// <summary>
        /// Urovo devices (official ScanManager docs). Current docs name the byte
        /// extra "barcode"; older firmware famously uses the typo "barocode" -
        /// both are tried. Note: Urovo defaults to keyboard output; intent mode
        /// must be enabled on the device (switchOutputMode 0).
        /// </summary>
        public static IntentScannerProfile Urovo => new()
        {
            Actions = ["android.intent.ACTION_DECODE_DATA"],
            DataExtraKeys = ["barcode_string"],
            ByteArrayExtraKeys = ["barocode", "barcode"],
            LengthExtraKey = "length",
        };

        /// <summary>
        /// Datalogic intent wedge (official SDK docs, defaults - remappable on
        /// device). The wedge is DISABLED by default; enable intent output in
        /// Datalogic Settings or via their SDK/OEMConfig.
        /// </summary>
        public static IntentScannerProfile Datalogic => new()
        {
            Actions = ["com.datalogic.decodewedge.decode_action"],
            DataExtraKeys = ["com.datalogic.decode.intentwedge.barcode_string"],
            ByteArrayExtraKeys = ["com.datalogic.decode.intentwedge.barcode_data"],
            FormatExtraKey = "com.datalogic.decode.intentwedge.barcode_type",
        };

        /// <summary>
        /// CipherLab reader service (official KB). Whether Decoder_Data arrives as
        /// String or byte[] is not documented - both are tried.
        /// </summary>
        public static IntentScannerProfile CipherLab => new()
        {
            Actions = ["com.cipherlab.barcodebaseapi.PASS_DATA_2_APP"],
            DataExtraKeys = ["Decoder_Data"],
            ByteArrayExtraKeys = ["Decoder_Data"],
            FormatExtraKey = "Decoder_CodeType_String",
        };

        /// <summary>
        /// Unitech USS scan service (official programming manual). Requires
        /// scan2key=false on the device. Symbology arrives on a separate broadcast
        /// (unitech.scanservice.datatype) and is not captured by this profile.
        /// </summary>
        public static IntentScannerProfile Unitech => new()
        {
            Actions = ["unitech.scanservice.data"],
            DataExtraKeys = ["text"],
        };

        /// <summary>
        /// Sunmi scan-head devices (vendor-documented defaults, configurable).
        /// Ensure the device output mode includes broadcast, not keystroke-only.
        /// </summary>
        public static IntentScannerProfile Sunmi => new()
        {
            Actions = ["com.sunmi.scanner.ACTION_DATA_CODE_RECEIVED"],
            DataExtraKeys = ["data"],
            ByteArrayExtraKeys = ["source_byte"],
        };

        /// <summary>
        /// Newland PDAs. Community-reported convention (cross-vendor integrations),
        /// not verified against official Newland docs - confirm on device.
        /// </summary>
        public static IntentScannerProfile Newland => new()
        {
            Actions = ["nlscan.action.SCANNER_RESULT"],
            DataExtraKeys = ["SCAN_BARCODE1", "SCAN_BARCODE2"],
        };

        /// <summary>
        /// iData PDAs. Community-reported (pre-2024 firmware); broadcast mode must
        /// be enabled on the device - confirm on device.
        /// </summary>
        public static IntentScannerProfile IData => new()
        {
            Actions = ["android.intent.action.SCANRESULT"],
            DataExtraKeys = ["value"],
        };

        /// <summary>
        /// Every fixed-default vendor preset - register the lot for an APK that
        /// works on whichever supported handheld it lands on.
        /// </summary>
        public static IntentScannerProfile[] AllKnown =>
            [Rt150, Urovo, Datalogic, CipherLab, Unitech, Sunmi, Newland, IData];
    }
}

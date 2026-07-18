namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// A single barcode scan: the code, which transport delivered it, when, and
    /// the symbology/format if the scanner reported one.
    /// </summary>
    public sealed class ScanResult(string barcode, ScanSource source = ScanSource.External, string? format = null)
    {
        public string Barcode { get; } = barcode;
        public ScanSource Source { get; } = source;
        public string? Format { get; } = format;
        public DateTime Timestamp { get; } = DateTime.Now;
    }
}

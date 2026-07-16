namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// A single barcode scan: the code, which transport delivered it, and when.
    /// </summary>
    public sealed class ScanResult(string barcode, ScanSource source = ScanSource.External)
    {
        public string Barcode { get; } = barcode;
        public ScanSource Source { get; } = source;
        public DateTime Timestamp { get; } = DateTime.Now;
    }
}

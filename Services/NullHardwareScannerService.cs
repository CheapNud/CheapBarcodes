namespace CheapBarcodes.Services
{
    /// <summary>
    /// No-op scanner for platforms without RT150 hardware (e.g. Windows desktop).
    /// Image scanning and generation still work; the hardware card just stays inactive.
    /// </summary>
    public class NullHardwareScannerService : IHardwareScannerService
    {
        public event EventHandler<string> HardwareBarcodeScanned = delegate { };

        public bool IsScanning => false;

        public void OnScan(string barcode) => HardwareBarcodeScanned.Invoke(this, barcode);

        public void StartScanning()
        {
        }

        public void StopScanning()
        {
        }
    }
}

namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// No-op scanner for platforms without RT150 hardware (e.g. Windows desktop).
    /// OnScan still raises the event, so scans can be simulated in tests.
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

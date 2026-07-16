namespace CheapBarcodes.Scanning
{
    public interface IHardwareScannerService
    {
        event Action<ScanResult> ScanReceived;
        void OnScan(ScanResult scan);
        void StartScanning();
        void StopScanning();
        bool IsScanning { get; }
    }
}

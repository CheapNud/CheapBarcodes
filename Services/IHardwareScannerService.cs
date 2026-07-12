namespace CheapBarcodes.Services
{
    public interface IHardwareScannerService
    {
        event EventHandler<string> HardwareBarcodeScanned;
        void OnScan(string barcode);
        void StartScanning();
        void StopScanning();
        bool IsScanning { get; }
    }
}

namespace CheapBarcodes.Scanning
{
    /// <summary>
    /// No-op scanner for platforms without RT150 hardware (e.g. Windows desktop).
    /// OnScan still raises the event, so scans from other transports (keyboard
    /// wedge, camera, tests) flow through the same pipeline.
    /// </summary>
    public class NullHardwareScannerService : IHardwareScannerService
    {
        public event Action<ScanResult> ScanReceived = delegate { };

        public bool IsScanning => false;

        public void OnScan(ScanResult scan) => ScanReceived.Invoke(scan);

        public void StartScanning()
        {
        }

        public void StopScanning()
        {
        }
    }
}

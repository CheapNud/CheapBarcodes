# CheapBarcodes.Scanning

RT150 handheld barcode scanner integration for Android, UI-agnostic. Wraps the CN.Pda serial-port SDK (via CheapBarcodes.Binding) plus the vendor broadcast fallback behind two small types:

- `IHardwareScannerService` / `AndroidHardwareScannerService` — scan event stream with beep + vibration feedback (`NullHardwareScannerService` for non-Android targets).
- `Rt150ScannerHost` — activity-lifecycle host for the scan thread and receivers.

## Usage

Register the service (MAUI shown; any Android DI works):

```csharp
#if ANDROID
builder.Services.AddSingleton<IHardwareScannerService, AndroidHardwareScannerService>();
#else
builder.Services.AddSingleton<IHardwareScannerService, NullHardwareScannerService>();
#endif
```

Wire the host into your MainActivity:

```csharp
private Rt150ScannerHost _scannerHost;

protected override void OnCreate(Bundle savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    _scannerHost = new Rt150ScannerHost(this);
    _scannerHost.BarcodeScanned += barcode =>
    {
        var scannerService = /* resolve IHardwareScannerService */;
        scannerService?.OnScan(barcode);
    };
}

protected override void OnStart() { base.OnStart(); _scannerHost.Start(); }
protected override void OnResume() { base.OnResume(); _scannerHost.Start(); }
protected override void OnPause() { UnhookIfNeeded(); _scannerHost.Stop(); base.OnPause(); }
protected override void OnDestroy() { _scannerHost.Dispose(); base.OnDestroy(); }
```

Then consume scans anywhere via `IHardwareScannerService.HardwareBarcodeScanned`.

The RT150's native libraries (`libdevapi.so`, `libirdaSerialPort.so`, armeabi-v7a) and `scan.jar` ship via the CheapBarcodes.Binding dependency — no manual jniLibs setup needed.

See the [CheapBarcodes](https://github.com/CheapNud/CheapBarcodes) demo app for a working MAUI Blazor frontend.

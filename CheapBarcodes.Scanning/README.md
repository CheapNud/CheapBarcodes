# CheapBarcodes.Scanning

RT150 handheld barcode scanner integration for Android, UI-agnostic. Wraps the CN.Pda serial-port SDK (via CheapBarcodes.Binding) plus the vendor broadcast fallback behind two small types:

- `IHardwareScannerService` / `AndroidHardwareScannerService` — scan event stream with beep + vibration feedback (`NullHardwareScannerService` for non-Android targets).
- `Rt150ScannerHost` — activity-lifecycle host for the scan thread and receivers.
- `KeyboardWedgeDetector` — platform-neutral detector for USB/Bluetooth HID scanners that type like keyboards (fast burst + Enter). Works on any platform, including desktop workstations.

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

## Keyboard-wedge (HID) scanners

Most budget USB and Bluetooth scanners present as keyboards. Register a `KeyboardWedgeDetector`, route its scans into the same pipeline, and feed it key events:

```csharp
builder.Services.AddSingleton(sp =>
{
    var detector = new KeyboardWedgeDetector();   // MaxInterKeyGap / MinBarcodeLength are tunable
    detector.BarcodeScanned += code => sp.GetService<IHardwareScannerService>()?.OnScan(code);
    return detector;
});
```

On Android, observe keys at activity level (works regardless of UI focus):

```csharp
public override bool DispatchKeyEvent(KeyEvent e)
{
    _detector?.ProcessKeyEvent(e);   // extension method, never consumes the event
    return base.DispatchKeyEvent(e);
}
```

On other platforms, feed `ProcessCharacter(char)` / `ProcessTerminator()` from whatever key source the UI has (e.g. a focused input's keydown events). Human typing is filtered out by burst timing.

The RT150's native libraries (`libdevapi.so`, `libirdaSerialPort.so`, armeabi-v7a) and `scan.jar` ship via the CheapBarcodes.Binding dependency — no manual jniLibs setup needed.

See the [CheapBarcodes](https://github.com/CheapNud/CheapBarcodes) demo app for a working MAUI Blazor frontend.

# CheapBarcodes.Scanning

UI-agnostic barcode scanning toolkit for .NET: every common scanner transport behind one `ScanResult` stream, plus GS1/GTIN parsing that also runs server-side. Bring your own frontend:

- `IHardwareScannerService` / `AndroidHardwareScannerService` — `ScanReceived` event stream of `ScanResult` (barcode + transport + timestamp) with beep + vibration feedback (`NullHardwareScannerService` for non-Android targets).
- `Rt150ScannerHost` — activity-lifecycle host for the RT150 scan thread and receivers.
- `IntentScannerHost` — generic broadcast-intent host for DataWedge/Honeywell-style devices: configure the action and extra key, get the same `ScanResult` stream.
- `KeyboardWedgeDetector` — platform-neutral detector for USB/Bluetooth HID scanners that type like keyboards (fast burst + Enter). Works on any platform, including desktop workstations.
- `Gs1Parser` / `Gtin` — pure string logic (no Android needed): GS1-128/DataMatrix element strings decomposed into application identifiers, and GTIN/EAN/UPC check-digit validation + GTIN-14 normalization.

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
    _scannerHost.ScanReceived += scan =>
    {
        var scannerService = /* resolve IHardwareScannerService */;
        scannerService?.OnScan(scan);
    };
}

protected override void OnStart() { base.OnStart(); _scannerHost.Start(); }
protected override void OnResume() { base.OnResume(); _scannerHost.Start(); }
protected override void OnPause() { UnhookIfNeeded(); _scannerHost.Stop(); base.OnPause(); }
protected override void OnDestroy() { _scannerHost.Dispose(); base.OnDestroy(); }
```

Then consume scans anywhere via `IHardwareScannerService.ScanReceived` — each `ScanResult` tells you the barcode, which transport delivered it (`SerialPort`, `Broadcast`, `KeyboardWedge`, `External`), and when.

## Other scanner brands (broadcast intents)

Most non-RT150 handhelds (Zebra DataWedge, Honeywell, Urovo, budget vendors) broadcast scans as an intent. `IntentScannerHost` takes one or more `IntentScannerProfile`s - register several and one APK works on whichever device it lands on:

```csharp
// Multi-device: whichever vendor's broadcast fires, wins
_scannerHost = new IntentScannerHost(this,
    IntentScannerProfile.Rt150,
    IntentScannerProfile.Urovo,   // byte[] payload + length extra handled
    new IntentScannerProfile      // Zebra DataWedge - action comes from your DataWedge profile
    {
        Actions = ["com.mycompany.ACTION"],
        DataExtraKeys = ["com.symbol.datawedge.data_string"],
        FormatExtraKey = "com.symbol.datawedge.label_type",
    });
_scannerHost.ScanReceived += scan => scannerService?.OnScan(scan);
```

Profiles support string extras (tried in order), byte-array extras with a length extra and configurable encoding (Chinese-market devices often use GBK), and an optional format/symbology extra that flows into `ScanResult.Format`. Any `Context` works as the host - Activity, Application, or a foreground Service for background scanning.

Same lifecycle wiring as `Rt150ScannerHost` (Start/Stop/Dispose). The single-action `(context, action, extraKey)` constructor still exists for the trivial case.

## Keyboard-wedge (HID) scanners

Most budget USB and Bluetooth scanners present as keyboards. Register a `KeyboardWedgeDetector`, route its scans into the same pipeline, and feed it key events:

```csharp
builder.Services.AddSingleton(sp =>
{
    var detector = new KeyboardWedgeDetector();   // MaxInterKeyGap / MinBarcodeLength are tunable
    detector.BarcodeScanned += code =>
        sp.GetService<IHardwareScannerService>()?.OnScan(new ScanResult(code, ScanSource.KeyboardWedge));
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

## GS1 / GTIN helpers

Server-side friendly (plain `net11.0`) — useful anywhere product barcodes are matched:

```csharp
// GS1-128 element strings: (01) GTIN, (10) batch, (17) expiry, (21) serial...
if (Gs1Parser.TryParse(scan.Barcode, out var gs1))
{
    var gtin = gs1.Gtin;              // "04006381333931"
    var expiry = gs1.ExpiryDate;      // DateOnly, end-of-month and century rules applied
    var batch = gs1.BatchOrLot;
}

// Plain EAN/UPC/GTIN product codes
if (Gtin.TryNormalize(scan.Barcode, out var gtin14))
{
    // check digit verified; EAN-13/UPC-A/GTIN-14 variants all normalize to the same key
}
```

FNC1/GS separators (ASCII 29) and symbology prefixes (`]C1`, `]d2`, ...) are handled. The AI table covers the common warehouse set; codes with unknown AIs fail parsing rather than guessing. A scan that fails both helpers is a custom/internal code.

## Logging

All hosts and the Android service accept an optional `Microsoft.Extensions.Logging.ILogger` (hosts via the `Logger` init property, the service via constructor injection) - without one the library is silent, including in Release builds. Wire whatever sink stack your app uses; the demo app ships Serilog with a rolling local file plus an optional Seq server.

The RT150's native libraries (`libdevapi.so`, `libirdaSerialPort.so`, armeabi-v7a) and `scan.jar` ship via the CheapBarcodes.Binding dependency — no manual jniLibs setup needed.

See the [CheapBarcodes](https://github.com/CheapNud/CheapBarcodes) demo app for a working MAUI Blazor frontend.

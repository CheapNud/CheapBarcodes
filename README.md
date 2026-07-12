# CheapBarcodes

A .NET MAUI Blazor hybrid application for Android and Windows that provides comprehensive barcode scanning and generation capabilities, with support for hardware barcode scanners (RT150 devices) and image-based scanning.

The app doubles as the demo/test frontend for **CheapBarcodes.Scanning**, the reusable UI-agnostic RT150 scanning library in this repo — consume that package if you need hardware scanning with your own UI.

## Features

### Hardware Barcode Scanning
- **RT150 Device Support**: Native integration with RT150 handheld scanners via serial port communication
- **Hardware Button Integration**: Supports function keys (F1-F5) for triggering scans
- **Dual Scanning Methods**:
  - Primary: SerialPort scanning (9600 baud, Com0)
  - Fallback: BroadcastReceiver intent-based scanning
- **Audio & Haptic Feedback**: System sound playback and vibration confirmation on successful scans
- **Lifecycle Management**: Automatic start/stop/pause handling with Android activity lifecycle

### Image Barcode Scanning
- **File Upload**: Support for .jpg, .jpeg, .png, .bmp, and .gif image formats
- **Async Processing**: Non-blocking image analysis with progress indicators
- **Multiple Format Detection**: Automatic barcode format recognition

### Barcode Generation
- **Text-to-Barcode**: Generate barcodes from any text input
- **Customizable Dimensions**: Width (50-500px) and height (20-200px) configuration
- **Format**: Outputs JPEG images as base64 data URIs for inline display
- **Live Preview**: Instant barcode preview after generation

### Scan History & Management
- **Persistent History**: Tracks up to 100 most recent scans
- **Detailed Records**: Timestamp, barcode data, format, and source (Hardware/Image)
- **Data Grid Display**: Sortable, filterable table view with MudBlazor DataGrid
- **Copy to Clipboard**: One-click copy for any barcode in history
- **Clear History**: Batch delete functionality

### User Interface
- **Material Design**: Built with MudBlazor 9.7.0 component library
- **Responsive Layout**: Adaptive cards for different screen sizes
- **Real-time Status**: Live scanning state indicators
- **Toast Notifications**: Success/error messages with ISnackbar

## Technology Stack

| Component | Version | Purpose |
|-----------|---------|---------|
| .NET | 11.0 (preview) | Runtime framework |
| Target Frameworks | net11.0-android, net11.0-windows | Platform targeting |
| MAUI | 10.0.80 | Cross-platform app framework |
| Blazor WebView | 10.0.80 | Hybrid web UI rendering |
| MudBlazor | 9.7.0 | Material Design component library |
| CheapHelpers.Services | 3.6.0 | Barcode services (generation, image scanning) |

### Android Requirements
- **Minimum SDK**: API 24 (Android 7.0)
- **Target SDK**: API 36 (Android 16)
- **Architecture**: armeabi-v7a (32-bit ARM)

### Permissions
```xml
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.VIBRATE" />
<uses-permission android:name="android.permission.INTERNET" />
```

## Project Structure

```
CheapBarcodes/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor           # MudLayout with AppBar and Drawer
│   │   └── NavMenu.razor              # Navigation menu
│   ├── Pages/
│   │   ├── Home.razor                 # Welcome page
│   │   └── Scanner.razor              # Main barcode scanner interface
│   ├── Shared/
│   │   └── ProgressButton.razor       # Reusable async button component
│   ├── Routes.razor                   # Blazor router configuration
│   └── _Imports.razor                 # Global using directives
│
├── Services/
│   ├── ApiUploadOptions.cs            # Phone-home settings (Preferences/SecureStorage)
│   └── ScanApiClient.cs               # Posts scans to the configured endpoint
│
├── Platforms/Android/
│   ├── MainActivity.cs                # Thin lifecycle wiring around Rt150ScannerHost
│   ├── MainApplication.cs             # Application class
│   └── AndroidManifest.xml            # Permissions and configuration
│
├── CheapBarcodes.Scanning/            # Reusable RT150 scanning library (NuGet)
│   ├── IHardwareScannerService.cs     # Scanner interface
│   ├── NullHardwareScannerService.cs  # No-op for non-Android targets
│   └── Android/                       # AndroidHardwareScannerService, Rt150ScannerHost, receivers
│
├── CheapBarcodes.Binding/             # CN.Pda scan.jar binding + native libs (NuGet)
│
├── wwwroot/                           # Blazor static assets
├── Resources/                         # MAUI resources (fonts, images, icons)
├── MauiProgram.cs                     # Service registration & app configuration
└── MainPage.xaml                      # BlazorWebView host page
```

## Hardware Scanning Architecture

### Flow Diagram
```
Hardware Scanner Button (F1-F5)
    ↓
KeyReceiver.OnReceive() → ProcessKeyCode
    ↓
TriggerScan() → ScanThread.Scan() (CN.Pda.Scan)
    ↓
Barcode Data → BarcodeReceiver.OnReceive()
    ↓
Handler message → MainActivity.OnScanMessage()
    ↓
AndroidHardwareScannerService.OnScan()
    ↓
Play sound + vibrate + Fire HardwareBarcodeScanned event
    ↓
Scanner.razor → ProcessScannedBarcode()
```

### Native Integration
Scanner integration lives in the reusable `CheapBarcodes.Scanning` library, which uses the Java interop bindings (CheapBarcodes.Binding) to communicate with:
- **CN.Pda.Scan.ScanThread**: Controls scanner hardware via serial port
- **CN.Pda.Serialport.SerialPort**: Low-level serial communication (9600 baud)

Native libraries (`libdevapi.so`, `libirdaSerialPort.so`) provide ARM-native implementations for RT150 devices.

## Installation & Setup

### Prerequisites
1. Visual Studio 2022 (17.14+) or Visual Studio Code
2. .NET 11 SDK (preview)
3. Android SDK (API 24-36)
4. RT150 barcode scanner device (for hardware scanning)

### Build Instructions

#### Visual Studio
1. Clone the repository
2. Open `CheapBarcodes.slnx`
3. Restore NuGet packages
4. Set build configuration to Debug/Release
5. Select Android emulator or physical device
6. Build solution (Ctrl+Shift+B)
7. Deploy to device (F5)

#### Command Line
```bash
# Clone repository
git clone <repository-url>
cd CheapBarcodes

# Restore dependencies
dotnet restore

# Build project
dotnet build -c Release -f net11.0-android

# Deploy to connected device
dotnet build -c Release -f net11.0-android -t:Run
```

### Configuration
The app automatically configures:
- MudBlazor theme provider with snackbar positioning
- IBarcodeService singleton registration
- Debug logging (in DEBUG builds)

## Usage

### Starting Hardware Scanning
1. Launch the app on an RT150 device
2. Navigate to "Scanner" page
3. Hardware scanning starts automatically on app launch
4. Use Start/Stop buttons to manually control scanning state
5. Press hardware function keys (F1-F5) to trigger scans
6. Scanned barcodes appear in "Last Scanned" card with audio/haptic feedback

### Scanning from Images
1. Navigate to "Scanner" page
2. Click "Select Image" under "Image Scanner" card
3. Choose an image file (.jpg, .png, .bmp, .gif)
4. Wait for processing
5. Result appears in "Last Scanned" card

### Generating Barcodes
1. Navigate to "Scanner" page
2. Enter text in "Text to encode" field under "Barcode Generator"
3. Optionally adjust Width (50-500px) and Height (20-200px)
4. Click "Generate Barcode"
5. Barcode image displays below the button

### Viewing Scan History
- All scans automatically appear in "Scan History" table
- Click copy icon to copy barcode to clipboard
- Click "Clear History" to delete all records
- History persists across app restarts (up to 100 scans); use Export CSV to share it

## API Reference

### IHardwareScannerService
Interface for hardware scanner integration (HardwareScannerService.cs:6-16).

```csharp
public interface IHardwareScannerService
{
    event EventHandler<string> HardwareBarcodeScanned;
    void OnScan(string barcode);
    void StartScanning();
    void StopScanning();
    bool IsScanning { get; }
}
```

**Events:**
- `HardwareBarcodeScanned`: Fired when a barcode is successfully scanned

**Methods:**
- `StartScanning()`: Activates hardware scanner and registers receivers
- `StopScanning()`: Deactivates scanner and unregisters receivers
- `OnScan(string barcode)`: Processes scanned barcode data (triggers sound/vibration)

**Properties:**
- `IsScanning`: Current scanning state

### IBarcodeService (CheapHelpers.Services)
Interface for barcode generation and image scanning.

```csharp
// Generate barcode from text (async)
Task<byte[]> GetBarcodeAsync(string input, int height = 30, int width = 100, BarcodeFormat format = BarcodeFormat.CODE_39, CancellationToken cancellationToken = default)

// Scan barcode from image (async)
Task<(string Text, string Format)?> ReadBarcodeAsync(byte[] imageBytes, CancellationToken cancellationToken = default)

// Event when barcode is detected
event Func<string, Task> BarcodeScanned
```

**Usage Example (Scanner.razor:361-365):**
```csharp
var buffer = new byte[file.Size];
await file.OpenReadStream().ReadAsync(buffer);

var scanResult = await BarcodeService.ReadBarcodeAsync(buffer);
if (scanResult.HasValue && !string.IsNullOrEmpty(scanResult.Value.Text))
{
    ProcessScannedBarcode(scanResult.Value.Text, scanResult.Value.Format, "Image");
}
```

### Scanner Page Component
Main UI component (Components/Pages/Scanner.razor).

**Key Methods:**
- `OnHardwareBarcodeScanned(object sender, string barcode)`: Hardware scan event handler (Line 285)
- `OnImageSelected(InputFileChangeEventArgs e)`: Image upload handler (Line 344)
- `GenerateBarcode()`: Barcode generation method (Line 390)
- `ProcessScannedBarcode(string barcode, string format, string source)`: Universal scan processor (Line 303)
- `AddToHistory(string barcode, string format, string source)`: History tracking (Line 315)

**Data Model (ScanRecord, Line 441-447):**
```csharp
public class ScanRecord
{
    public string Barcode { get; set; }
    public string Format { get; set; }
    public string Source { get; set; }     // "Hardware" or "Image"
    public DateTime Timestamp { get; set; }
}
```

## Development Notes

### Service Registration
Services are registered in `MauiProgram.cs:22-54`:
- HttpClient (2-minute timeout)
- MudServices (snackbar configuration)
- IBarcodeService singleton

### Android Lifecycle Integration
MainActivity handles scanner lifecycle (MainActivity.cs:25-128):
- **OnCreate**: Initialize handler, wait for MAUI, start scanning
- **OnStart/OnResume**: Register broadcast receivers
- **OnPause**: Unregister receivers, pause scanning
- **OnDestroy**: Stop scanning, clean up resources

### Event Handling
The app uses multiple event patterns:
- **EventHandler&lt;string&gt;**: Hardware scanner events
- **Func&lt;string, Task&gt;**: Async barcode service events
- **Handler.Callback**: Android message passing

### Known Limitations
1. **RT150 Specific**: Hardware scanning requires RT150 devices (Android); on Windows the hardware scanner card stays inactive
2. **No iOS/macOS**: Android and Windows only

## Contributing
When contributing, ensure:
- Follow existing code style and patterns
- Use MudBlazor components for UI (avoid HTML tags)
- Add XML documentation comments for public APIs
- Test on physical RT150 devices when modifying scanner code
- Update this README for new features

## License
[Specify license here]

## Support
For issues or questions:
- File issues in the project repository
- Check MudBlazor documentation: https://mudblazor.com/
- Review MAUI documentation: https://learn.microsoft.com/dotnet/maui/

---

**Project Status**: Production-ready for hardware scanning | Image scanning feature pending implementation

<!--
  TODO.md — CheapBarcodes project work tracker
  Last updated: 2026-07-18

  RULES FOR AI AGENTS:
  - Update the "Last updated" date above whenever you modify this file
  - Items use checkbox format: - [ ] incomplete, - [x] complete
  - Never remove completed items — they serve as history. Move them to "## Done" when a category gets cluttered.
  - Each item gets ONE line. Details go in sub-bullets indented with 2 spaces.
  - Prefix each item with the date it was added: - [ ] (2026-03-17) Description
  - When completing, change to: - [x] (2026-03-17 → 2026-03-18) Description
  - Tag the SOURCE of each item at the end in brackets:
      [code-todo] = from // TODO comment in source code
      [plan] = from a plan document or planning session
      [bug] = from a bug encountered during dev/deploy
      [audit] = from a code audit or review
      [user] = explicitly requested by the user
  - For [code-todo] items, ALWAYS include file:line reference so devs can navigate directly
  - Categories: Blocking, Planned, Future, Done
  - New items go at the TOP of their category
  - Do not create separate TODO_*.md files — everything goes here
  - Keep it terse. If it needs more than 3 sub-bullets, link to a plan document.
  - Do NOT create, rename, or remove categories — the fixed set is: Blocking, Planned, Future, Done
  - When asked for planned work or TODO analysis, ALWAYS include Future items too — list them below Planned and note them as future work
-->

# TODO

## Blocking

_Nothing blocking._

## Planned

- [ ] (2026-07-11) Distinct error sound — second MediaPlayer buzz for failures (no barcode found, API post failed) [audit]
- [ ] (2026-07-11) Show app version on Home via VersionTracking.CurrentVersion [audit]

## Future

- [ ] (2026-07-11) WakeLock during scanning — deferred: device display-timeout config covered this for years in production; revisit only if RT150 testing shows serial drops when the screen dims [audit]
- [ ] (2026-07-11) Camera-based scanning mode so the app works on ordinary phones, not just RT150 (BarcodeScanning.Native.Maui or ZXing camera view) [audit]
- [ ] (2026-07-11) Barcode format picker + QR support in the generator — service defaults to CODE_39, ZXing already encodes the rest [audit]
- [ ] (2026-07-11) Crash telemetry via Sentry MAUI SDK (AppCenter is retired) — optional, evaluate need first [audit]

## Done

- [x] (2026-07-16 → 2026-07-18) GS1 parser + GTIN/EAN validation in CheapBarcodes.Scanning 2.1.0 — Gs1Parser/Gs1Barcode/Gtin, 27 xunit tests gating the publish workflow, GS1 chips on the demo scan card (PR #10) [user]
  - Known GS1 ambiguity: element strings aren't self-identifying, a plain EAN starting with a valid AI parses as GS1 — symbology prefix is the reliable discriminator
- [x] (2026-07-11 → 2026-07-16) OAuth add-on for API phone-home — client-credentials with SecureStorage secret, cached token, refresh-on-401; activates when a token endpoint is set (PR #9) [user]
- [x] (2026-07-11 → 2026-07-16) Offline queue for API phone-home — bounded Preferences queue, drains in order before new posts / on reconnect / via settings retry button (PR #9) [audit]
- [x] (2026-07-11 → 2026-07-16) Duplicate-scan guard — DROPPED: scanner hardware config (same-code re-read delay) covers this; no code needed [user]
- [x] (2026-07-16 → 2026-07-16) Scanning 2.0.0: ScanResult event payload (barcode/transport/timestamp) + IntentScannerHost for DataWedge/Honeywell-style broadcast scanners (PR #8) [user]
  - Breaking event-shape change done deliberately before external consumers exist; demo history Source column now shows the real transport
- [x] (2026-07-13 → 2026-07-13) Keyboard-wedge (HID) scanner support in CheapBarcodes.Scanning — many budget handhelds present as USB/Bluetooth keyboards instead of serial/intent [user]
  - KeyboardWedgeScannerHost: activity forwards DispatchKeyEvent, burst-timing heuristic separates scanner bursts from human typing, Enter (or configurable suffix) terminates → same BarcodeScanned event
  - Feeds the existing IHardwareScannerService pipeline, so consumers don't care which transport delivered the scan
  - Testable without scanner hardware: fast paste/typing on the Windows demo build simulates a wedge burst
  - Shipped in CheapBarcodes.Scanning 1.1.0 (PR #7): KeyboardWedgeDetector + Android DispatchKeyEvent adapter + demo wedge input field (Enter/Tab terminated)
- [x] (2026-07-12 → 2026-07-12) Split UI from scanning core — CheapBarcodes.Scanning library (NuGet-ready, UI-agnostic, no MAUI dependency) + app demoted to demo/test frontend (PR #5) [user]
  - Rt150ScannerHost wraps scan thread + receivers behind activity lifecycle calls; scan.jar + native libs ship inside the CheapBarcodes.Binding package
  - Published 1.0.0 to nuget.org via tag-triggered nuget-publish.yml (tag v1.0.0)
- [x] (2026-07-11 → 2026-07-11) Windows desktop target — net11.0-windows TFM, scanner service split into interface + Android impl + desktop no-op, binding/native libs gated to Android (PR #4) [user]
  - Also fixed IHardwareScannerService never being registered in DI (lost in the November migration — scanner page was broken at runtime)
- [x] (2026-07-11 → 2026-07-11) Persist scan history + CSV export/share — history currently dies with the app session [audit]
  - Preferences + JSON of last 100 records; Export CSV button shares via the Android share sheet (PR #3)
- [x] (2026-07-11 → 2026-07-11) Fold CheapBarcodes.Binding into this repo — was an unversioned sibling folder, briefly its own GitHub repo (now archived) [user]
- [x] (2026-07-11 → 2026-07-11) Configurable API phone-home — post scans to a user-configured endpoint so the app adapts to any environment [user]
  - Settings page (Preferences-backed): base URL, auth header name + value (API key/bearer), auto-post on/off, test-connection button
  - POST JSON per scan: barcode, format, source, timestamp, device name; reuse the registered singleton HttpClient (currently unused — do NOT remove it)
  - Legacy reference: MecamApplication.Handheld posted to hardcoded WebServiceBase endpoints — this replaces that pattern with runtime config
- [x] (2026-07-11 → 2026-07-11) Fix receiver registration for Android 14+ — RegisterReceiver without RECEIVER_EXPORTED/NOT_EXPORTED throws SecurityException on API 34+, silently swallowed [audit]
  - MainActivity.cs:169,190,198 — vendor broadcasts (BARCODEPORT_RECEIVEDDATA_ACTION, FUN_KEY) originate outside the app, so likely need Exported; verify on RT150
- [x] (2026-07-11 → 2026-07-11) Fix DI race in MainActivity.OnCreate — Task.Delay(100) hope-based service resolution drops hardware scans if MAUI boots slower [audit]
  - Resolve lazily in OnScanMessage with ??= like the legacy Handheld did (MainActivity.cs:37-46)
- [x] (2026-07-11 → 2026-07-11) Fix image upload partial-read — single ReadAsync doesn't guarantee a full buffer and OpenReadStream caps at 500KB [audit]
  - Scanner.razor:357-358 — use OpenReadStream(maxAllowedSize) + CopyToAsync into a MemoryStream
- [x] (2026-07-11 → 2026-07-11) Fix history format column for hardware scans — always says "Hardware" instead of the barcode format [audit]
  - Scanner.razor:288 — intent extras carry no format; display "Unknown" or decode-side detection
- [x] (2026-07-11 → 2026-07-11) Remove dead weight: MvvmCross.Plugin.Messenger (only supplies MvxMessage for one const), rename ScanMesasge.cs → ScanMessage.cs, drop unused CAMERA permission [audit]
  - Replace ScanMessage with a plain class holding const int Scan = 1001
  - CAMERA comes back if/when camera scanning (Future) lands

- [x] (2026-07-11 → 2026-07-11) Unpin CheapHelpers.Services and enable image scanning [user]
  - CheapHelpers 3.6.0 dropped the Microsoft.AspNetCore.App framework reference from CheapHelpers.EF, so the NETSDK1082 Android blocker is gone
  - Migrated Scanner.razor: `ReadBarcodeAsync(bytes)` (width/height args dropped), `GetBarcode` → `await GetBarcodeAsync`; image scanning now functional
- [x] (2026-07-11 → 2026-07-11) Migrate solution to slnx format like sibling repos [user]
- [x] (2026-07-11 → 2026-07-11) Bring repo in line with sibling projects: net11.0-android + MAUI 10.0.80 + MudBlazor 9.7.0, CodeQL (buildless) + dependency review workflows, README refresh [user]

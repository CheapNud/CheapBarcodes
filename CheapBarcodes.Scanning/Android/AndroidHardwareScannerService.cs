using Android.Media;
using Microsoft.Extensions.Logging;
using A = Android; //stupid but resolves namespace conflicts

namespace CheapBarcodes.Scanning
{
    public class AndroidHardwareScannerService : IHardwareScannerService, IDisposable
    {
        private readonly ILogger<AndroidHardwareScannerService>? _logger;
        private MediaPlayer? _mediaPlayer;
        private bool _isScanning;

        public event Action<ScanResult> ScanReceived = delegate { };
        public bool IsScanning => _isScanning;

        public AndroidHardwareScannerService(ILogger<AndroidHardwareScannerService>? logger = null)
        {
            _logger = logger;
            _isScanning = false;
            InitializeMediaPlayer();
        }

        private void InitializeMediaPlayer()
        {
            try
            {
                // Try to use a system notification sound for scan confirmation
                var appContext = A.App.Application.Context;

                // First try to create with notification sound
                _mediaPlayer = MediaPlayer.Create(appContext, A.Provider.Settings.System.DefaultNotificationUri);

                // If that fails, try ringtone
                _mediaPlayer ??= MediaPlayer.Create(appContext, A.Provider.Settings.System.DefaultRingtoneUri);

                // If still null, we'll just skip sound
                if (_mediaPlayer == null)
                {
                    _logger?.LogWarning("Could not initialize MediaPlayer - no sound will be played");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to initialize MediaPlayer");
                _mediaPlayer = null; // Ensure it's null so we don't try to use it
            }
        }

        public void OnScan(ScanResult scan)
        {
            if (string.IsNullOrEmpty(scan?.Barcode))
                return;

            try
            {
                _logger?.LogInformation("Barcode scanned ({Source}): {Barcode}", scan.Source, scan.Barcode);

                // Play sound notification (Android-specific feature)
                PlayScanSound();

                // Provide haptic feedback (Android-specific feature)
                ProvideHapticFeedback();

                // Notify subscribers
                ScanReceived.Invoke(scan);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing hardware barcode");
            }
        }

        public void StartScanning()
        {
            _isScanning = true;
            _logger?.LogInformation("Hardware barcode scanning started");
        }

        public void StopScanning()
        {
            _isScanning = false;
            _logger?.LogInformation("Hardware barcode scanning stopped");
        }

        private void PlayScanSound()
        {
            try
            {
                if (_mediaPlayer != null && _isScanning)
                {
                    // Reset if already playing
                    if (_mediaPlayer.IsPlaying)
                    {
                        _mediaPlayer.Stop();
                        _mediaPlayer.Prepare();
                    }
                    _mediaPlayer.Start();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error playing scan sound");
            }
        }

        private void ProvideHapticFeedback()
        {
            try
            {
                var appContext = A.App.Application.Context;

                // Provide haptic feedback on scan
                var vibrator = appContext.GetSystemService(A.Content.Context.VibratorService) as A.OS.Vibrator;
                if (vibrator != null && vibrator.HasVibrator)
                {
                    if (A.OS.Build.VERSION.SdkInt >= A.OS.BuildVersionCodes.O)
                    {
                        vibrator.Vibrate(A.OS.VibrationEffect.CreateOneShot(100, A.OS.VibrationEffect.DefaultAmplitude));
                    }
                    else
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        vibrator.Vibrate(100);
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error providing haptic feedback");
            }
        }

        public void Dispose()
        {
            try
            {
                _isScanning = false;

                if (_mediaPlayer != null)
                {
                    if (_mediaPlayer.IsPlaying)
                    {
                        _mediaPlayer.Stop();
                    }
                    _mediaPlayer.Release();
                    _mediaPlayer.Dispose();
                    _mediaPlayer = null;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error disposing AndroidHardwareScannerService");
            }
        }
    }
}

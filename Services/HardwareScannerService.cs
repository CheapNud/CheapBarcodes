// File: Services/IHardwareScannerService.cs
using Android.Media;
using CheapBarcodes.Services;
using A = Android; //stupid but resolves namespace conflicts

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

namespace CheapBarcodes.Platforms.Android.Services
{
    public class AndroidHardwareScannerService : IHardwareScannerService, IDisposable
    {
        private MediaPlayer _mediaPlayer;
        private bool _isScanning;

        public event EventHandler<string> HardwareBarcodeScanned;
        public bool IsScanning => _isScanning;

        public AndroidHardwareScannerService()
        {
            _isScanning = false;
            InitializeMediaPlayer();
        }

        private void InitializeMediaPlayer()
        {
            try
            {
                // Try to use a system notification sound for scan confirmation
                var context = Platform.CurrentActivity ?? A.App.Application.Context;

                // First try to create with notification sound
                _mediaPlayer = MediaPlayer.Create(context, A.Provider.Settings.System.DefaultNotificationUri);

                // If that fails, try ringtone
                if (_mediaPlayer == null)
                {
                    _mediaPlayer = MediaPlayer.Create(context, A.Provider.Settings.System.DefaultRingtoneUri);
                }

                // If still null, we'll just skip sound
                if (_mediaPlayer == null)
                {
                    System.Diagnostics.Debug.WriteLine("Could not initialize MediaPlayer - no sound will be played");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize MediaPlayer: {ex.Message}");
                _mediaPlayer = null; // Ensure it's null so we don't try to use it
            }
        }

        public void OnScan(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"Hardware barcode scanned: {barcode}");

                // Play sound notification (Android-specific feature)
                PlayScanSound();

                // Provide haptic feedback (Android-specific feature)
                ProvideHapticFeedback();

                // Notify subscribers
                HardwareBarcodeScanned?.Invoke(this, barcode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing hardware barcode: {ex.Message}");
            }
        }

        public void StartScanning()
        {
            _isScanning = true;
            System.Diagnostics.Debug.WriteLine("Hardware barcode scanning started");
        }

        public void StopScanning()
        {
            _isScanning = false;
            System.Diagnostics.Debug.WriteLine("Hardware barcode scanning stopped");
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
                System.Diagnostics.Debug.WriteLine($"Error playing scan sound: {ex.Message}");
            }
        }

        private void ProvideHapticFeedback()
        {
            try
            {
                var context = Platform.CurrentActivity;
                if (context != null)
                {
                    // Provide haptic feedback on scan
                    var vibrator = context.GetSystemService(A.Content.Context.VibratorService) as A.OS.Vibrator;
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error providing haptic feedback: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"Error disposing AndroidHardwareScannerService: {ex.Message}");
            }
        }
    }
}
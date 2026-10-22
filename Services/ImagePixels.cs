using System.Diagnostics;
using ZXing;
#if ANDROID
using Android.Graphics;
#elif WINDOWS
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
#endif

namespace CheapBarcodes.Services
{
    /// <summary>
    /// Decodes an image file (JPEG, PNG, ...) to raw RGBA pixels with the platform's own decoder,
    /// so barcode reading needs no native image library.
    /// </summary>
    public static class ImagePixels
    {
        private const int BytesPerPixel = 4;

#if ANDROID
        // ZXing gains nothing above this, and a 12 MP photo would be 48 MB of RGBA
        private const int MaxDecodedSide = 2048;
#endif

        /// <summary>
        /// Returns the pixels row by row without padding, or null when the file cannot be decoded.
        /// </summary>
        public static async Task<(byte[] Pixels, int Width, int Height, RGBLuminanceSource.BitmapFormat Format)?> DecodeAsync(byte[] encoded)
        {
#if ANDROID
            // BitmapFactory is CPU work; keep it off the UI thread
            return await Task.Run(() => DecodeAndroid(encoded));
#elif WINDOWS
            try
            {
                using var stream = new InMemoryRandomAccessStream();
                await stream.WriteAsync(encoded.AsBuffer());
                stream.Seek(0);

                var decoder = await BitmapDecoder.CreateAsync(stream);
                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Rgba8,
                    BitmapAlphaMode.Ignore,
                    new BitmapTransform(),
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage);

                return (pixelData.DetachPixelData(), (int)decoder.PixelWidth, (int)decoder.PixelHeight, RGBLuminanceSource.BitmapFormat.RGBA32);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not decode image: {ex.Message}");
                return null;
            }
#endif
        }

#if ANDROID
        private static (byte[] Pixels, int Width, int Height, RGBLuminanceSource.BitmapFormat Format)? DecodeAndroid(byte[] encoded)
        {
            var bounds = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeByteArray(encoded, 0, encoded.Length, bounds);

            var sampleSize = 1;
            while (Math.Max(bounds.OutWidth, bounds.OutHeight) / sampleSize > MaxDecodedSide)
                sampleSize *= 2;

            var options = new BitmapFactory.Options
            {
                InPreferredConfig = Bitmap.Config.Argb8888,
                InSampleSize = sampleSize
            };

            var bitmap = BitmapFactory.DecodeByteArray(encoded, 0, encoded.Length, options);
            if (bitmap is null)
            {
                Debug.WriteLine("Could not decode image");
                return null;
            }

            try
            {
                // Some sources (16-bit PNG, indexed GIF) ignore the preferred config
                if (bitmap.GetConfig() != Bitmap.Config.Argb8888)
                {
                    var converted = bitmap.Copy(Bitmap.Config.Argb8888!, false);
                    bitmap.Recycle();
                    bitmap = converted;
                    if (bitmap is null)
                        return null;
                }

                var width = bitmap.Width;
                var height = bitmap.Height;
                var rowBytes = bitmap.RowBytes;

                var raw = new byte[rowBytes * height];
                var nativeBuffer = Java.Nio.ByteBuffer.Allocate(raw.Length);
                bitmap.CopyPixelsToBuffer(nativeBuffer);
                nativeBuffer.Rewind();
                nativeBuffer.Get(raw);

                // ARGB_8888 is stored as R,G,B,A in memory; ZXing wants rows without stride padding
                var packedRow = width * BytesPerPixel;
                if (rowBytes == packedRow)
                    return (raw, width, height, RGBLuminanceSource.BitmapFormat.RGBA32);

                var pixels = new byte[packedRow * height];
                for (var row = 0; row < height; row++)
                    Buffer.BlockCopy(raw, row * rowBytes, pixels, row * packedRow, packedRow);

                return (pixels, width, height, RGBLuminanceSource.BitmapFormat.RGBA32);
            }
            finally
            {
                bitmap?.Recycle();
            }
        }
#endif
    }
}

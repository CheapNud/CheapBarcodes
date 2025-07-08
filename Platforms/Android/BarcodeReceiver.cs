using Android.Content;
using Android.OS;
using CheapBarcodes.Helpers;

namespace CheapBarcodes.Platforms.Android
{
    public class BarcodeReceiver : BroadcastReceiver
    {
        private readonly Handler _handler;

        public BarcodeReceiver(Handler handler)
        {
            _handler = handler;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent?.Extras == null)
            {
                System.Diagnostics.Debug.WriteLine("BarcodeReceiver received null intent or extras.");
                return;
            }

            string barcode = intent.Extras.GetString("DATA");
            if (barcode == null)
            {
                System.Diagnostics.Debug.WriteLine("BarcodeReceiver received null barcode.");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"BarcodeReceiver received barcode: {barcode}");

            // Convert bundle to use the same data param
            Bundle bundle = intent.Extras;
            bundle.PutString("data", barcode);

            // Create message using modern pattern
            Message msg = Message.Obtain();
            msg.What = ScanMessage._scan;
            msg.Data = bundle;

            _handler.SendMessage(msg);
        }
    }
}
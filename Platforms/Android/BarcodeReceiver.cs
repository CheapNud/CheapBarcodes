using Android.Content;
using Android.OS;
using CheapBarcodes.Helpers;

namespace CheapBarcodes.Platforms.Android
{
    public class BarcodeReceiver : BroadcastReceiver
    {
        public BarcodeReceiver(Handler handler)
        {
            _handler = handler;
        }
        readonly Handler _handler;
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent?.Extras == null)
            {
                System.Diagnostics.Debug.WriteLine("Barcodereceiver received null intent or extras.");
                return;
            }

            string? barcode = intent.Extras.GetString("DATA");
            if (barcode == null)
            {
                System.Diagnostics.Debug.WriteLine("Barcodereceiver received null barcode.");
                return;
            }

            System.Diagnostics.Debug.WriteLine($@"Barcodereceiver received barcode: {barcode}");
            //convert bundle to use the same data param
            Bundle bundle = intent.Extras;
            bundle.PutString("data", barcode);
            Message msg = new Message()
            {
                What = ScanMessage._scan,
                Data = bundle
            };
            _handler.SendMessage(msg);
        }
    }
}
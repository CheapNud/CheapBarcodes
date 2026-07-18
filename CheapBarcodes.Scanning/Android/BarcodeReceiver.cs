using Android.Content;
using Android.OS;
using Microsoft.Extensions.Logging;

namespace CheapBarcodes.Scanning
{
    internal class BarcodeReceiver : BroadcastReceiver
    {
        internal const int ScanMessageId = 1001;

        private readonly Handler _handler;
        private readonly ILogger? _logger;

        public BarcodeReceiver(Handler handler, ILogger? logger = null)
        {
            _handler = handler;
            _logger = logger;
        }

        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent?.Extras == null)
            {
                _logger?.LogDebug("BarcodeReceiver received null intent or extras");
                return;
            }

            string? barcode = intent.Extras.GetString("DATA");
            if (barcode == null)
            {
                _logger?.LogDebug("BarcodeReceiver received null barcode");
                return;
            }

            _logger?.LogDebug("BarcodeReceiver received barcode: {Barcode}", barcode);

            // Convert bundle to use the same data param; mark the transport so the
            // host can distinguish broadcast scans from serial scan-thread messages
            Bundle bundle = intent.Extras;
            bundle.PutString("data", barcode);
            bundle.PutString("transport", "broadcast");

            Message msg = Message.Obtain();
            msg.What = ScanMessageId;
            msg.Data = bundle;

            _handler.SendMessage(msg);
        }
    }
}

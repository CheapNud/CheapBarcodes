using Android.Content;
using Android.Views;
using CN.Pda.Scan;
using System;

namespace CheapBarcodes.Platforms.Android
{
    public class KeyReceiver : BroadcastReceiver
    {
        public KeyReceiver(ScanThread scanThread)
        {
            _scanThread = scanThread;
        }

        private ScanThread _scanThread;
        TimeSpan _timeout = TimeSpan.FromSeconds(1.8);
        DateTime _startTime = DateTime.Now;

        public override void OnReceive(Context context, Intent intent)
        {
            if (DateTime.Now - _startTime > _timeout)
            {
                if (intent.GetBooleanExtra("keydown", false))
                {
                    int keyCode = intent.GetIntExtra("keyCode", 0);
                    switch ((Keycode)keyCode)
                    {
                        case Keycode.F1:
                            System.Diagnostics.Debug.WriteLine(keyCode + " F1");
                            // _scanThread.scan();
                            _startTime = DateTime.Now;
                            break;
                        case Keycode.F2:
                            System.Diagnostics.Debug.WriteLine(keyCode + " F2");
                            // scanThread.scan();
                            _startTime = DateTime.Now;
                            break;
                        case Keycode.F3:
                            System.Diagnostics.Debug.WriteLine(keyCode + " F3 Pistol Key");
                            _scanThread.Scan();
                            _startTime = DateTime.Now;
                            break;
                        case Keycode.F4:
                            System.Diagnostics.Debug.WriteLine(keyCode + " F4");
                            // scanThread.scan();
                            _startTime = DateTime.Now;
                            break;
                        case Keycode.F5:
                            System.Diagnostics.Debug.WriteLine(keyCode + " F5");
                            // scanThread.scan();
                            _startTime = DateTime.Now;
                            break;
                    }
                }
            }
        }
    }
}
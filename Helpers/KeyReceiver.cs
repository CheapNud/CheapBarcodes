using Android.Content;
using Android.Views;
using CN.Pda.Scan;
using System;

namespace MecamMobile.Android.Helpers
{
    public class KeyReceiver : BroadcastReceiver
    {
        public KeyReceiver(ScanThread scanThread)
        {
            _scanThread = scanThread;
        }

        private ScanThread _scanThread;
        TimeSpan timeout = TimeSpan.FromSeconds(1.8);
        DateTime startTime = DateTime.Now;

        public override void OnReceive(Context context, Intent intent)
        {
            if (DateTime.Now - startTime > timeout)
            {
                if (intent.GetBooleanExtra("keydown", false))
                {
                    int keyCode = intent.GetIntExtra("keyCode", 0);
                    switch ((Keycode)keyCode)
                    {
                        case Keycode.F1:
                            System.Diagnostics.Debug.WriteLine(keyCode + " F1");
                            // _scanThread.scan();
                            startTime = DateTime.Now;
                            break;
                        case Keycode.F2:
                            System.Diagnostics.Debug.WriteLine(keyCode + " F2");
                            // scanThread.scan();
                            startTime = DateTime.Now;
                            break;
                        case Keycode.F3:
                            System.Diagnostics.Debug.WriteLine(keyCode + " F3 Pistol Key");
                            _scanThread.Scan();
                            startTime = DateTime.Now;
                            break;
                        case Keycode.F4:
                            System.Diagnostics.Debug.WriteLine(keyCode + " F4");
                            // scanThread.scan();
                            startTime = DateTime.Now;
                            break;
                        case Keycode.F5:
                            System.Diagnostics.Debug.WriteLine(keyCode + " F5");
                            // scanThread.scan();
                            startTime = DateTime.Now;
                            break;
                    }
                }
            }
        }
    }
}
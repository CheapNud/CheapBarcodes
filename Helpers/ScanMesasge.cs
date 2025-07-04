using Android.OS;
using MvvmCross.Plugin.Messenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheapBarcodes.Helpers
{
    public class ScanMessage : MvxMessage
    {
        public static readonly int _scan = 1001;

        public ScanMessage(object sender, string scanData) : base(sender)
        {
            ScanData = scanData.Replace("\r", string.Empty);
        }

        public string ScanData { get; set; }
    }
}
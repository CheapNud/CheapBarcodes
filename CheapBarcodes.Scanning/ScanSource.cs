namespace CheapBarcodes.Scanning
{
    public enum ScanSource
    {
        /// <summary>Fed by the consumer (camera library, test, simulation).</summary>
        External = 0,

        /// <summary>RT150-class serial-port scan thread.</summary>
        SerialPort = 1,

        /// <summary>Vendor broadcast intent (RT150 fallback, DataWedge-style scanners).</summary>
        Broadcast = 2,

        /// <summary>USB/Bluetooth HID scanner typing like a keyboard.</summary>
        KeyboardWedge = 3,
    }
}

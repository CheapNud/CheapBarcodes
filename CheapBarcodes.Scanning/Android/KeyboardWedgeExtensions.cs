using Android.Views;

namespace CheapBarcodes.Scanning
{
    public static class KeyboardWedgeExtensions
    {
        /// <summary>
        /// Feed an Android key event into the detector - call from the activity's
        /// DispatchKeyEvent override, then continue with base.DispatchKeyEvent so
        /// the UI still receives the keys. Only key-down events are considered.
        /// </summary>
        public static void ProcessKeyEvent(this KeyboardWedgeDetector detector, KeyEvent? keyEvent)
        {
            if (keyEvent == null || keyEvent.Action != KeyEventActions.Down)
            {
                return;
            }

            if (keyEvent.KeyCode is Keycode.Enter or Keycode.NumpadEnter)
            {
                detector.ProcessTerminator();
                return;
            }

            var inputChar = (char)keyEvent.UnicodeChar;
            if (inputChar != '\0' && !char.IsControl(inputChar))
            {
                detector.ProcessCharacter(inputChar);
            }
        }
    }
}

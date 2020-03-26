using Android.App;
using Android.Content;

namespace LiveDisplay.Servicios.Keyguard
{
    public class KeyguardHelper
    {
        private static KeyguardManager myKM = (KeyguardManager)Application.Context.GetSystemService(Context.KeyguardService);

        public static bool IsSystemSecured()
        {
            if (myKM.IsKeyguardSecure)
                return true;
            return false;
        }
        public static bool IsDeviceCurrentlyLocked()
        {
            KeyguardManager myKM = (KeyguardManager)Application.Context.GetSystemService(Context.KeyguardService);
            if (myKM.IsKeyguardLocked)
                return true;
            return false;
        }
    }
}
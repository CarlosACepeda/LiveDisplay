using Android.App;
using Android.Content;
using Android.Hardware.Fingerprints;
using Android.OS;

namespace LiveDisplay.Services.Keyguard
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
            if (myKM.IsKeyguardLocked)
                return true;
            return false;
        }

        public static bool IsFingerprintSet()
        {
            FingerprintManager myFM = (FingerprintManager)Application.Context.GetSystemService(Context.FingerprintService);
            if (myFM.IsHardwareDetected == false) return false;

            return myFM.HasEnrolledFingerprints;
        }

        internal static void RequestDismissKeyguard(Activity activity)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                myKM.RequestDismissKeyguard(activity, null);
        }
    }
}
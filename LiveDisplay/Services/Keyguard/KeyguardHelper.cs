using Android.App;
using Android.Content;
using Android.Hardware.Fingerprints;
using Android.OS;
using System;
using System.Threading;

namespace LiveDisplay.Services.Keyguard
{
    public class KeyguardHelper: KeyguardManager.KeyguardDismissCallback
    {
        private static KeyguardManager myKM = (KeyguardManager)Application.Context.GetSystemService(Context.KeyguardService);

        const int MaxWaitTimeInMillis = 10000;
        const int HalfSecondInMillis = 500;

        public event EventHandler<bool> KeyguardDismissed;

        public override void OnDismissSucceeded()
        {
            KeyguardDismissed?.Invoke(this, true);
            base.OnDismissSucceeded();
        }
        public override void OnDismissCancelled()
        {
            KeyguardDismissed?.Invoke(this, false);
            base.OnDismissCancelled();
        }
        public override void OnDismissError()
        {
            KeyguardDismissed?.Invoke(this, false);
            base.OnDismissError();
        }

        public bool IsDeviceCurrentlyLocked()
        {
            return myKM.IsDeviceLocked;
        }
        public bool IsFingerprintSet()
        {
            FingerprintManager myFM= (FingerprintManager)Application.Context.GetSystemService(Context.KeyguardService);
            if (myFM.IsHardwareDetected == false) return false;

            return myFM.HasEnrolledFingerprints;
        }
        public void RequestKeyguardDismissal(Activity activityOnTopOfKeyguard)
        {
            if (!IsDeviceCurrentlyLocked()) OnDismissSucceeded();
            else
            {

                if (Build.VERSION.SdkInt <= BuildVersionCodes.NMr1)
                {
                    activityOnTopOfKeyguard.Window.AddFlags(Android.Views.WindowManagerFlags.DismissKeyguard);
                    ListenForKeyguardDismissalNougat();
                }
                else
                {
                    myKM.RequestDismissKeyguard(activityOnTopOfKeyguard, this);
                }
            }
        }

        void ListenForKeyguardDismissalNougat()
        {
            int actualMillis = 0;
            ThreadPool.QueueUserWorkItem(m =>
            {
                while (actualMillis <= MaxWaitTimeInMillis)
                {
                    Thread.Sleep(HalfSecondInMillis);
                    actualMillis += HalfSecondInMillis;
                    if (!IsDeviceCurrentlyLocked())
                    {
                        OnDismissSucceeded();
                        return;
                    }
                    
                }
                if (IsDeviceCurrentlyLocked())
                    OnDismissCancelled();
                else OnDismissSucceeded();
            });
        }
    }
}
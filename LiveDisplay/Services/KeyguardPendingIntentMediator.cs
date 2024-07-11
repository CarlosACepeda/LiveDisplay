using Android.App;
using Android.Content;
using Android.OS;
using Java.Lang;
using LiveDisplay.Misc;
using LiveDisplay.Services.Keyguard;
using System;
using System.Threading;

namespace LiveDisplay.Services
{
    public class KeyguardPendingIntentMediator
    {
        Activity activityRequestingKeyguardDismissal;
        KeyguardHelper keyguardHelper;
        PendingIntent pendingIntent;

        public event EventHandler<KeyguardPendingIntentMediator> RequiredSetActivityToBeCalled; //Activities Showing on the Lock Screen should listen to this event
        //to make itself present so the next functionalities work.

         static KeyguardPendingIntentMediator instance;

        private KeyguardPendingIntentMediator()
        {
            keyguardHelper = new KeyguardHelper();
            keyguardHelper.KeyguardDismissed += KeyguardHelper_KeyguardDismissed;
        }

        public static KeyguardPendingIntentMediator GetInstance()
        {
            instance ??= new KeyguardPendingIntentMediator();
            return instance;
        }

        public void SetActivity(Activity activityRequestingKeyguardDismissal)
        {
            this.activityRequestingKeyguardDismissal = activityRequestingKeyguardDismissal;
            RequestDismissKeyboard();
        }

        private void KeyguardHelper_KeyguardDismissed(object sender, bool e)
        {
            if (e)
            {
                pendingIntent.Send(); //TODO: Doesn't Send due to Background Activity Launch restrictions (Android Q+), try to find a workaround.
                activityRequestingKeyguardDismissal.MoveTaskToBack(true);
            }
            else
            {
                Console.WriteLine("Keyguard wasn't dismissed, nothing will be done");
            }
        }

        public void SendPendingIntent(PendingIntent pendingIntent)
        {
            this.pendingIntent = pendingIntent;

            RequiredSetActivityToBeCalled?.Invoke(this, this);
        }
        void RequestDismissKeyboard()
        {
           keyguardHelper.RequestKeyguardDismissal(activityRequestingKeyguardDismissal);
        }
    }
}
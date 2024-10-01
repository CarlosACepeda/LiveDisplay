using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using LiveDisplay.Services.Keyguard;
using System;

namespace LiveDisplay.Services
{
    public class KeyguardPendingIntentMediator: Java.Lang.Object, PendingIntent.IOnFinished
    {
        Activity activityRequestingKeyguardDismissal;
        readonly KeyguardHelper keyguardHelper;
        PendingIntent pendingIntent;

        readonly int ModeBackgroundActivityStartAllowed, 
            ModeBackgroundActivityStartAllowedByPermission,
            PendingIntentCreatorBackgroundActivityStartMode = 1;

        Bundle BALSkipOptions;

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

        private void KeyguardHelper_KeyguardDismissed(object sender, bool wasDismissed)
        {
            if (wasDismissed)
            {
                try
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Q) //BAL is applied since Android Q(API 29) but really enforced as of Android 14 (API 34)
                    {
                        //We pass this Data, but the only thing we really require is the IOnFinished interface, to skip the
                        //Background Activity Launch restrictions.
                        //because this call will always fail if API level is +34

                        pendingIntent.Send(Application.Context, Result.FirstUser, null, this, null, string.Empty, null);
                    }
                    else
                    {
                        pendingIntent.Send(); //sweet and nice expected behavior when Android is not Q and up (opens the Activity this PendingIntent represents)
                        activityRequestingKeyguardDismissal.MoveTaskToBack(true);
                    }
                }
                catch (PendingIntent.CanceledException pice)
                {
                    Console.WriteLine(pice);
                }
                
            }
            else
            {
                Console.WriteLine("Keyguard wasn't dismissed, nothing will be done");
            }
        }

        public void SendPendingIntent(PendingIntent pendingIntent)
        {
            this.pendingIntent = pendingIntent;
            this.BALSkipOptions = SetRequiredBALPermissionsBundle();

            RequiredSetActivityToBeCalled?.Invoke(this, this);
        }

        private Bundle SetRequiredBALPermissionsBundle()
        {
            var activityOptions = ActivityOptions.MakeBasic();
            var bundle = activityOptions.ToBundle();
            bundle.PutInt("android.activity.pendingIntentCreatorBackgroundActivityStartMode", PendingIntentCreatorBackgroundActivityStartMode);
            bundle.PutInt("android.pendingIntent.backgroundActivityAllowed", ModeBackgroundActivityStartAllowed);
            bundle.PutInt("android.pendingIntent.backgroundActivityAllowedByPermission", ModeBackgroundActivityStartAllowedByPermission);

            return bundle;
        }

        void RequestDismissKeyboard()
        {
           keyguardHelper.RequestKeyguardDismissal(activityRequestingKeyguardDismissal);
        }

        public void OnSendFinished(PendingIntent pendingIntent, Intent intent, [GeneratedEnum] Result resultCode, string resultData, Bundle resultExtras)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                //From here simply we get the intent that was part of the PendingIntent that failed due to BAL restrictions
                //and attach the BAL skip options to it
                //as final step, start the activity as if we created this intent.
                //really cool workaround.
                activityRequestingKeyguardDismissal.StartActivity(intent, BALSkipOptions);
                activityRequestingKeyguardDismissal.MoveTaskToBack(true);
            }
        }
    }
}
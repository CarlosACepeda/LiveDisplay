using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Java.Lang;
using Java.Nio.Channels;
using LiveDisplay.Misc;
using LiveDisplay.Services.Keyguard;
using System;

namespace LiveDisplay.Services
{
    public class KeyguardPendingIntentMediator: Java.Lang.Object, PendingIntent.IOnFinished
    {
        Activity activityRequestingKeyguardDismissal;
        readonly KeyguardHelper keyguardHelper;
        PendingIntent pendingIntent;
        PendingIntent alternatePendingIntent;
        readonly bool ModeBackgroundActivityStartAllowed, ModeBackgroundActivityStartAllowedByPermission = true;
        readonly int  PendingIntentCreatorBackgroundActivityStartMode = 1;
        int requestCode = -1;

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
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                    {
                        try
                        {
                            //BAL is applied since Android Q(API 29) but really enforced as of Android 14 (API 34)
                            //We pass this Data, but the only thing we really require is the IOnFinished interface, to skip the
                            //Background Activity Launch restrictions.
                            //because this call will always fail (but doesn't throw any exception) if API level is +34 
                            pendingIntent.Send(Application.Context, Result.FirstUser, null, this, null, string.Empty, BALSkipOptions);
                        }
                        catch (PendingIntent.CanceledException pice)
                        {
                            Console.WriteLine($"Main PendingIntent failed,sending alternate if provided: ({alternatePendingIntent != null}) {pice}");
                            alternatePendingIntent?.Send(Application.Context, Result.FirstUser, null, this, null, string.Empty, BALSkipOptions);
                        }
                    }
                    else
                    {
                        try
                        {
                            pendingIntent.Send(); //sweet and nice expected behavior when Android is not Q and up (opens the Activity this PendingIntent represents)
                        }
                        catch (PendingIntent.CanceledException pice)
                        {
                            Console.WriteLine($"Main PendingIntent failed,sending alternate if provided: ({alternatePendingIntent!=null}) {pice}");
                            alternatePendingIntent?.Send();
                        }

                    }

                }
                catch (Java.Lang.Exception ex)
                {
                    Console.WriteLine($"All PendingIntent send tries failed, {ex}");
                }
            }
            else
            {
                Console.WriteLine("Keyguard wasn't dismissed, nothing will be done");
            }
        }

        public void SendPendingIntent(PendingIntent pendingIntent, PendingIntent alternatePendingIntent= null)
        {
            this.pendingIntent = pendingIntent;
            this.alternatePendingIntent = alternatePendingIntent;
            BALSkipOptions = SetRequiredBALPermissionsBundle();

            RequiredSetActivityToBeCalled?.Invoke(this, this);
        }

        private Bundle SetRequiredBALPermissionsBundle()
        {
            var activityOptions = ActivityOptions.MakeBasic();
            var bundle = activityOptions.ToBundle();
            bundle.PutInt("android.activity.pendingIntentCreatorBackgroundActivityStartMode", PendingIntentCreatorBackgroundActivityStartMode);
            bundle.PutBoolean("android.pendingIntent.backgroundActivityAllowed", ModeBackgroundActivityStartAllowed);
            bundle.PutBoolean("android.pendingIntent.backgroundActivityAllowedByPermission", ModeBackgroundActivityStartAllowedByPermission);

            return bundle;
        }

        void RequestDismissKeyboard()
        {
           keyguardHelper.RequestKeyguardDismissal(activityRequestingKeyguardDismissal);
        }

        public void OnSendFinished(PendingIntent pendingIntent, Intent intent, [GeneratedEnum] Result resultCode, string resultData, Bundle resultExtras)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q && resultCode== Result.Canceled)
            {
                try
                {
                    //From here simply we get the intent that was part of the PendingIntent that failed due to BAL restrictions
                    //and attach the BAL skip options to it
                    //as final step, start the activity as if we created this intent.
                    //really cool workaround.
                    activityRequestingKeyguardDismissal.StartActivity(intent, BALSkipOptions);

                }
                catch (Java.Lang.Exception ex)
                {
                    //if the activity we are trying to start doesn't allow other components to start it from clicking the notification
                    //namely, this app, then we are gonna resort to creating a "good intent" which will take us to the Main Activity of the application
                    try
                    {
                        activityRequestingKeyguardDismissal.StartActivity(PackageUtils.GetAppIntent(intent.Package), BALSkipOptions);
                    }
                    catch
                    {
                        Console.WriteLine($"No way to start this application: {ex}");
                    }
                }
                finally
                {
                    activityRequestingKeyguardDismissal.MoveTaskToBack(true);
                }
            }
        }
    }
}
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using LiveDisplay.Activities;
using LiveDisplay.Services.Notifications;
using LiveDisplay.Services.Notifications.NotificationEventArgs;
using System;

namespace LiveDisplay.Services
{
    /// <summary>
    /// This class is to perform notification actions, such as clicking, removing and posting!
    /// </summary>
    internal class NotificationSlave : Java.Lang.Object, PendingIntent.IOnFinished
    {
        private NotificationManager notificationManager;
        private static NotificationSlave instance;

        public event EventHandler<NotificationCancelledEventArgsKitkat> NotificationCancelled;

        public event EventHandler<NotificationCancelledEventArgsLollipop> NotificationCancelledLollipop;
        public event EventHandler ResendLastNotificationRequested;

        public event EventHandler AllNotificationsCancelled;

        private NotificationSlave()
        {
           notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
        }
        public static NotificationSlave GetInstance()
        {
            if(instance== null)
            {
                instance = new NotificationSlave();
            }
            return instance;
        }
        public void CancelNotification(string notiPack, string notiTag, int notiId)
        {
            OnNotificationCancelled(new NotificationCancelledEventArgsKitkat
            {
                NotificationPackage = notiPack,
                NotificationTag = notiTag,
                NotificationId = notiId
            });
        }

        public void CancelNotification(string key)
        {
            OnNotificationCancelled(new NotificationCancelledEventArgsLollipop
            {
                Key = key
            });
        }

        public void CancelAll()
        {
            OnAllNotificationsCancelled();
        }

        public void PostNotification(int notifid, string title, string text, bool autoCancellable, NotificationPriority notificationPriority)
        {
            Android.App.Notification.Builder builder = new Android.App.Notification.Builder(Application.Context);
            builder.SetContentTitle(title);
            builder.SetContentText(text);
            builder.SetAutoCancel(autoCancellable);
            builder.SetPriority(Convert.ToInt32(notificationPriority));
            builder.SetSmallIcon(Resource.Drawable.ic_stat_default_appicon);
            notificationManager.Notify(notifid, builder.Build());
        }

        public void PostNotification(int notifid,string title, string text, bool autoCancellable, NotificationImportance notificationImportance)
        {
            NotificationChannel notificationChannel = new NotificationChannel("livedisplaynotificationchannel", "LiveDisplay", notificationImportance);
            notificationManager.CreateNotificationChannel(notificationChannel);
            Android.App.Notification.Builder builder = new Android.App.Notification.Builder(Application.Context, "livedisplaynotificationchannel");
            builder.SetContentTitle(title);
            builder.SetContentText(text);
            builder.SetAutoCancel(autoCancellable);
            builder.SetSmallIcon(Resource.Drawable.ic_stat_default_appicon);
            builder.SetAutoCancel(true);
            builder.SetStyle(new Android.App.Notification.MessagingStyle("CULO"));

            Android.App.RemoteInput remoteInput = new RemoteInput.Builder("test1").SetLabel("This is the place where you write").Build();

            Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(SettingsActivity)));

            PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, 35, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Mutable );

            Android.App.Notification.Action.Builder action = new Android.App.Notification.Action.Builder(Resource.Drawable.ic_stat_default_appicon, "Answer", pendingIntent).AddRemoteInput(remoteInput);

            builder.AddAction(action.Build());

            notificationManager.Notify(notifid, builder.Build());
        }

        public void SendDumbNotification()
        {
            Android.App.Notification.Builder builder;
            if (Build.VERSION.SdkInt < BuildVersionCodes.NMr1)
            {

                builder = new Android.App.Notification.Builder(Application.Context);
                builder.SetPriority(Convert.ToInt32(NotificationPriority.Max));
            }
            else
            {
                NotificationChannel notificationChannel = new NotificationChannel("livedisplaynotificationchannel", "LiveDisplay", NotificationImportance.Max);
                notificationManager.CreateNotificationChannel(notificationChannel);
                builder = new Android.App.Notification.Builder(Application.Context, "livedisplaynotificationchannel");
            }
            builder.SetContentTitle("");
            builder.SetContentText("");
            builder.SetAutoCancel(true);

            builder.SetSmallIcon(Resource.Drawable.ic_stat_default_appicon);
            notificationManager.Notify(2, builder.Build());
        }

        public void RetrieveLastNotification() //ask Catcher to resend the last notification posted, (In case it was missed)
        {
            ResendLastNotificationRequested?.Invoke(this, null);
        }

        //Raising events.
        protected virtual void OnNotificationCancelled(NotificationCancelledEventArgsKitkat e)
        {
            NotificationCancelled?.Invoke(this, e);
        }

        protected virtual void OnNotificationCancelled(NotificationCancelledEventArgsLollipop e)
        {
            NotificationCancelledLollipop?.Invoke(this, e);
        }

        protected virtual void OnAllNotificationsCancelled()
        {
            AllNotificationsCancelled?.Invoke(this, EventArgs.Empty);
        }

        internal void ClickNotification(OpenNotification notification)
        {
            try
            {
                var intent = notification.ContentIntent;
                intent ??= notification.FullScreenIntent;

                //This is part of a Workaround to make LockScreen show on Android Q devices and above:
                //Please check CatcherHelper#OnNotificationPosted() to get an idea of how it works.

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q && notification.PackageName == "com.underground.livedisplay" /*Only act on notifications sent by this app*/)
                {
                    //Workaround behavior.
                    //Causes a FullScreenIntent that's contained within a Notification matchig the if statement to be sent correctly.
                    //For some unknown reason the usual "Send()" method doesn't work if the screen is locked.
                    intent.Send(Result.Ok, this, new Handler());
                    CancelNotification(notification.Key); //ignoring documentation: if we leave this notification alive after performing the previous line intent.Send(...),
                                                          //then after if the same notification gets posted without the previous one being removed then the intent.Send(...) won't succeed.
                                                          //and the lockscreen won't show.
                                                          //Android is weird.

                }
                else
                {
                    //Usual behavior.

                    intent.Send();
                    //Android Docs: For NotificationListeners: When implementing a custom click for notification
                    //Cancel the notification after it was clicked when this notification is autocancellable.
                    if (notification.IsAutoCancellable)
                        CancelNotification(notification.Key);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Click Notification failed, fail in pending intent {ex.Message}");
            }
        }

        public void ClickAction(OpenAction action, string text= "")
        {
            if(text!= string.Empty)
            {
                //We must send the text through the pending intent this action has, used in Inline Responses from notification.
                Bundle bundle = new Bundle();
                Intent intent = new Intent();
                bundle.PutCharSequence(action.FirstRemoteInput.ResultKey, text);
                Android.App.RemoteInput.AddResultsToIntent(action.RemoteInputs, intent, bundle);
                action.ActionIntent.Send(Application.Context, Result.Ok, intent);
            }
            else
            {
                action.ActionIntent.Send();
            }
        }

        public void OnSendFinished(PendingIntent pendingIntent, Intent intent, [GeneratedEnum] Result resultCode, string resultData, Bundle resultExtras)
        {
            Console.WriteLine($"Android Q background activity launch was defeated by me (debug info, FullScreenIntent result):  {resultCode} || {resultData}");
        }
    }
}
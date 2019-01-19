using Android.App;
using Android.Content;
using LiveDisplay.Activities;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using System;

namespace LiveDisplay.Servicios
{
    //Esta clase sirve para manipular las notificaciones, como quitarlas o agregarlas.
    internal class NotificationSlave : Java.Lang.Object
    {
        private static NotificationSlave instance;

        public event EventHandler<NotificationCancelledEventArgsKitkat> NotificationCancelled;

        public event EventHandler<NotificationCancelledEventArgsLollipop> NotificationCancelledLollipop;

        public event EventHandler AllNotificationsCancelled;

        private NotificationSlave()
        {
        }

        public static NotificationSlave NotificationSlaveInstance()
        {
            if (instance == null)
            {
                instance = new NotificationSlave();
            }
            return instance;
        }

        //Postear Notificaciones sobre mi app.
        private NotificationManager notificationManager = (NotificationManager)Application.Context.GetSystemService("notification");

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

        public void PostNotification(string title, string text, bool autoCancellable, NotificationPriority notificationPriority)
        {
#pragma warning disable CS0618 // 'Notification.Builder(Context) está obsoleto
            Notification.Builder builder = new Notification.Builder(Application.Context);
#pragma warning restore
            builder.SetContentTitle(title);
            builder.SetContentText(text);
            builder.SetAutoCancel(autoCancellable);
#pragma warning disable CS0618 // 'Notification.Builder.SetPriority(int)' está obsoleto: 'deprecated'
            builder.SetPriority(Convert.ToInt32(NotificationPriority.Low));
#pragma warning restore CS0618 // 'Notification.Builder.SetPriority(int)' está obsoleto: 'deprecated'
            
            builder.SetSmallIcon(Resource.Drawable.ic_stat_default_appicon);
            notificationManager.Notify(1, builder.Build());
        }
        public void PostNotification(string title, string text, bool autoCancellable, NotificationImportance notificationImportance)
        {
            NotificationChannel notificationChannel = new NotificationChannel("livedisplaynotificationchannel", "LiveDisplay", notificationImportance);
            notificationManager.CreateNotificationChannel(notificationChannel);
            Notification.Builder builder = new Notification.Builder(Application.Context, "livedisplaynotificationchannel");
            builder.SetContentTitle(title);
            builder.SetContentText(text);
            builder.SetAutoCancel(autoCancellable);
            builder.SetSmallIcon(Resource.Drawable.ic_stat_default_appicon);

            RemoteInput remoteInput = new RemoteInput.Builder("test")
    .SetLabel("Your inline response").Build();

            Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(SettingsActivity)));

            PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, 35, intent, PendingIntentFlags.UpdateCurrent);

            Notification.Action.Builder action = new Notification.Action.Builder(Resource.Drawable.ic_stat_default_appicon, "Answer", pendingIntent).AddRemoteInput(remoteInput);

            builder.AddAction(action.Build());

            notificationManager.Notify(1, builder.Build());
        }
        public void SendDumbNotification()
        {

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
    }
}
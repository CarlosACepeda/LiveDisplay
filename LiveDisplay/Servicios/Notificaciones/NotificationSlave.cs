using Android.App;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using System;

namespace LiveDisplay.Servicios
{
    //Esta clase sirve para manipular las notificaciones, como quitarlas o agregarlas.
    internal class NotificationSlave : Java.Lang.Object
    {
        public static NotificationSlave instance;

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
            //For android Oreo I must Specify a NotificationChannel
            //USe SetPriority/SetImportance on Different Android devices.
            Notification.Builder builder = new Notification.Builder(Application.Context);
            builder.SetContentTitle(title);
            builder.SetContentText(text);
            builder.SetAutoCancel(autoCancellable);
#pragma warning disable CS0618 // 'Notification.Builder.SetPriority(int)' está obsoleto: 'deprecated'
            builder.SetPriority(Convert.ToInt32(NotificationPriority.Low));
#pragma warning restore CS0618 // 'Notification.Builder.SetPriority(int)' está obsoleto: 'deprecated'
            
            builder.SetSmallIcon(Resource.Drawable.ic_stat_default_appicon);
            notificationManager.Notify(1, builder.Build());
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
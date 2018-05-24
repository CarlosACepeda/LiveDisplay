using Android.App;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using System;

namespace LiveDisplay.Servicios
{
    //Esta clase sirve para manipular las notificaciones, como quitarlas o agregarlas.
    internal class NotificationSlave
    {

        public event EventHandler<NotificationCancelledEventArgsKitkat> NotificationCancelled;
        public event EventHandler<NotificationCancelledEventArgsLollipop> NotificationCancelledLollipop;
        public event EventHandler AllNotificationsCancelled;


        //Postear Notificaciones sobre mi app.
        private NotificationManager notificationManager = (NotificationManager)Application.Context.GetSystemService("notification");

        public void CancelNotification(string notiPack, string notiTag, int notiId)
        {
            NotificationCancelled(this, new NotificationCancelledEventArgsKitkat
            {
                NotificationPackage = notiPack,
                NotificationTag = notiTag,
                NotificationId = notiId
            });
        }

        public void CancelNotification(string key)
        {
            NotificationCancelledLollipop(this, new NotificationCancelledEventArgsLollipop
            {
                Key = key
            });
        }

        public void CancelAll()
        {
            AllNotificationsCancelled(this, new EventArgs());          
        }

        public void PostNotification()
        {
            //Test.
            Notification.Builder builder = new Notification.Builder(Application.Context);
            builder.SetContentTitle("LiveDisplay");
            builder.SetContentText("This is a test notification");
            builder.SetAutoCancel(true);
            builder.SetPriority(Convert.ToInt32(Android.App.NotificationPriority.Low));
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
        protected virtual void OnAllNotificationsCancelled(EventArgs e)
        {
            AllNotificationsCancelled?.Invoke(this, e);
        }
    }
}
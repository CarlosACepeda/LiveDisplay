using Android.App;
using Android.Content;
using Android.OS;
using Android.Service.Notification;
using Android.Util;

namespace LiveDisplay.Servicios
{
    [Service(Label = "Caaatcherr", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService"})]
    internal class Catcher : NotificationListenerService
    {
#pragma warning disable CS0649 // El campo 'Catcher.codes' nunca se asigna y siempre tendrá el valor predeterminado
        private BuildVersionCodes codes;
#pragma warning restore CS0649 // El campo 'Catcher.codes' nunca se asigna y siempre tendrá el valor predeterminado

        public delegate void OnNotificationPostedEventHandler(StatusBarNotification statusBarNotification);

        private LockScreenActivity lockScreenActivity = new LockScreenActivity();

        //Válido para Lollipop en Adelante, no KitKat.
        public override void OnListenerConnected()
        {
            base.OnListenerConnected();
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            OnNotificationPostedEventHandler eventHandler = new OnNotificationPostedEventHandler(lockScreenActivity.BuildNotification);
            eventHandler.Invoke(sbn);
        }

        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            Log.Info("leeel", "Notificación Removida");
        }
    }
}
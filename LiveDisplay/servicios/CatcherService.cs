using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Service.Notification;
using Android.Util;
using LiveDisplay.Databases;
using LiveDisplay.Misc;
using LiveDisplay.Objects;
using System.IO;
using static Android.Graphics.Bitmap;

namespace LiveDisplay.Servicios
{
    [Service(Label = "Catcherjer", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    internal class Catcher : NotificationListenerService
    {
        private DBHelper helper = new DBHelper();
        private ActivityLifecycleHelper lifecycleHelper = new ActivityLifecycleHelper();

        //Válido para Lollipop en Adelante, no KitKat.
        public override void OnListenerConnected()
        {
            base.OnListenerConnected();
            //implementar un método que recupere las notificaciones que estén en la barra de estado mientras el este Agente de escucha no estaba 'oyendo'

        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            ClsNotification notification = new ClsNotification
            {
                Id = sbn.Id,
                Titulo = sbn.Notification.Extras.GetCharSequence("android.title").ToString(),
                Texto = sbn.Notification.Extras.GetCharSequence("android.text").ToString(),
                Icono = int.Parse(sbn.Notification.Extras.Get(Notification.ExtraSmallIcon).ToString()),
                Paquete = sbn.PackageName
            };
            helper.InsertIntoTableNotification(notification);
            if (LockScreenActivity.lockScreenInstance != null)
            {
                LockScreenActivity.lockScreenInstance.RunOnUiThread(() => LockScreenActivity.lockScreenInstance.InsertItem(notification));
            }

        }

        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            ClsNotification notification = new ClsNotification
            {
                Id = sbn.Id,
                Titulo = null,
                Texto = null,
                Icono = 0
            };
            helper.DeleteTableNotification(notification);
            if (LockScreenActivity.lockScreenInstance != null)
            {
                LockScreenActivity.lockScreenInstance.RunOnUiThread(() => LockScreenActivity.lockScreenInstance.RemoveItem(notification));
            }
        }

        public byte[] BitmapToByteArray(Bitmap bitmap)
        {
            byte[] bitmapData;
            using (var stream = new MemoryStream())
            {
                bitmap.Compress(CompressFormat.Png, 0, stream);
                return bitmapData = stream.ToArray();
            }
        }
    }
}
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Service.Notification;
using Android.Util;
using Android.Widget;
using Java.IO;
using LiveDisplay.Adapters;
using LiveDisplay.Databases;
using LiveDisplay.Misc;
using LiveDisplay.Objects;
using System.IO;
using System.Threading;
using static Android.Graphics.Bitmap;

namespace LiveDisplay.Servicios
{
    [Service(Label = "Catcheeeer", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService"})]
    internal class Catcher : NotificationListenerService
    {
        DBHelper helper = new DBHelper();
        ActivityLifecycleHelper lifecycleHelper = new ActivityLifecycleHelper();

        //Válido para Lollipop en Adelante, no KitKat.
        public override void OnListenerConnected()
        {
            base.OnListenerConnected();
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
            Log.Info("Inserción", "Registro Insertado desde OnNoificationPosted");

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
            Log.Info("Remoción", "Notificación Removida desde OnNotificationRemoved");
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Service.Notification;
using Android.Util;
using Android.Graphics;
using System.IO;

namespace LiveDisplay.Servicios
{
    [Service(Label = "Catcherr", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    class Catcher: NotificationListenerService
    {
        BuildVersionCodes codes;
        String texto;

        //Válido para Lollipop en Adelante, no KitKat.
        public override void OnListenerConnected()
        {
            base.OnListenerConnected();
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            String nombrePaquete = sbn.PackageName;
            String ticker = "";
            Log.Info("leeel", "Notificación Posteada");
            if (sbn.Notification != null && codes.Equals(19))
            {
                ticker = sbn.Notification.TickerText.ToString();
            }
            Bundle extras = sbn.Notification.Extras;
            String titulo = extras.GetString("android.title");
            //try
            //{
            //     texto = extras.GetCharSequence("android.text").ToString();
            //    Log.Info("Exito", "Capturado");
            //}
            //catch
            //{
            //    Log.Info("Exception", "No se puede obtener el android.text");
            //}
            int id1 = extras.GetInt(Notification.ExtraSmallIcon);
            Bitmap id = sbn.Notification.LargeIcon;

            Intent intent = new Intent("notificationSender");
            intent.PutExtra("nombrePaquete", nombrePaquete);
            if (Convert.ToInt32(codes) <= 19)
            {
                intent.PutExtra("ticker", ticker);
            }
            intent.PutExtra("titulo", titulo);
            intent.PutExtra("texto", texto);
            if (id != null)
            {
                MemoryStream stream = new MemoryStream();
                id.Compress(Bitmap.CompressFormat.Png, 100, stream);
                byte[] byteArray = stream.ToArray();
                intent.PutExtra("icon", byteArray);
            }
            intent.SetAction("test.test");
            //LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(intent);
            SendBroadcast(intent);
            Log.Info("broadcast", "Broadcast enviado como notifSender");

        }
        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            Log.Info("leeel", "Notificación Removida");
        }
    }
}
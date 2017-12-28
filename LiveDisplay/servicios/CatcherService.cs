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

namespace LiveDisplay.servicios
{
    [Service(Label = "Catcherr", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    class Catcher: NotificationListenerService
    {
#pragma warning disable CS0649 // El campo 'Catcher.codes' nunca se asigna y siempre tendrá el valor predeterminado
        private BuildVersionCodes codes;
#pragma warning restore CS0649 // El campo 'Catcher.codes' nunca se asigna y siempre tendrá el valor predeterminado
#pragma warning disable CS0649 // El campo 'Catcher.texto' nunca se asigna y siempre tendrá el valor predeterminado null
        private String texto;
#pragma warning restore CS0649 // El campo 'Catcher.texto' nunca se asigna y siempre tendrá el valor predeterminado null

        //Válido para Lollipop en Adelante, no KitKat.
        public override void OnListenerConnected()
        {
            base.OnListenerConnected();
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            GetActiveNotifications();

            //String nombrePaquete = sbn.PackageName;
            //String ticker = "";
            //Log.Info("leeel", "Notificación Posteada");

            ////Solo en Kitkat hay Ticker, en >Lollipop no hay, retorna null.
            //if (sbn.Notification != null && codes.Equals(19))
            //{
            //    ticker = sbn.Notification.TickerText.ToString();
            //}
            //Bundle extras = sbn.Notification.Extras;
            //String titulo = extras.GetString("android.title");
            //texto = extras.GetString("android.summaryText");
            //int id1 = extras.GetInt(Notification.ExtraSmallIcon);
            //Bitmap id = sbn.Notification.LargeIcon;

            //Intent intent = new Intent("notificationSender");
            //intent.PutExtra("nombrePaquete", nombrePaquete);
            //if (Convert.ToInt32(codes) <= 19)
            //{
            //    intent.PutExtra("ticker", ticker);
            //}
            //intent.PutExtra("titulo", titulo);
            //intent.PutExtra("texto", texto);
            //if (id != null)
            //{
            //    MemoryStream stream = new MemoryStream();
            //    id.Compress(Bitmap.CompressFormat.Png, 100, stream);
            //    byte[] byteArray = stream.ToArray();
            //    intent.PutExtra("icon", byteArray);
            //}
            ////LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(intent);
            //SendBroadcast(intent);
            //Log.Info("broadcast", "Broadcast enviado como notifSender");

        }
        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            Log.Info("leeel", "Notificación Removida");
            
        }
        
    }
}
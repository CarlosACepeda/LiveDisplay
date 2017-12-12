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
using Android.Support.V4.Content;

namespace LiveDisplay.Servicios
{
    [Service(Label = "Catcher", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    class Catcher: NotificationListenerService
    {

        public override void OnListenerConnected()
        {
            base.OnListenerConnected();
        }
        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            base.OnNotificationPosted(sbn);
            String nombrePaquete = sbn.PackageName;
            String ticker = "";
            if (sbn.Notification != null)
            {
                ticker = sbn.Notification.TickerText.ToString();

            }
            Bundle extras = sbn.Notification.Extras;
            String titulo = extras.GetString("android.title");
            String texto = extras.GetCharSequence("android.text").ToString();
            int id1 = extras.GetInt(Notification.ExtraSmallIcon);
            Bitmap id = sbn.Notification.LargeIcon;

            Intent notifSender = new Intent("notificationSender");
            notifSender.PutExtra("nombrePaquete", nombrePaquete);
            notifSender.PutExtra("ticker", ticker);
            notifSender.PutExtra("titulo", titulo);
            notifSender.PutExtra("texto", texto);
            if (id != null)
            {
                MemoryStream stream = new MemoryStream();
                id.Compress(Bitmap.CompressFormat.Png, 100, stream);
                byte[] byteArray = stream.ToArray();
                notifSender.PutExtra("icon", byteArray);
            }
            LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(notifSender);

        }
        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            base.OnNotificationRemoved(sbn);
        }
    }
}
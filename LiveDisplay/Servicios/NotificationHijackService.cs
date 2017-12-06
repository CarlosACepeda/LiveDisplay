using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Service.Notification;
using Android.Graphics;
using System.IO;
using Android.Support.V4.Content;
using Android.Widget;

namespace LiveDisplay.Servicios
{
    class NotificationHijackService : NotificationListenerService 
    {

        static int sdkVersion = Convert.ToInt32(Build.VERSION.SdkInt);


        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            base.OnNotificationPosted(sbn);
            Toast.MakeText(Application.Context, "LEL", ToastLength.Short);
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
            Console.WriteLine("notificacion removida");
            Toast.MakeText(Application.Context, "LAL", ToastLength.Short);
        }

        public bool SupportsNotificationSettings()
        {
            if (sdkVersion >= 19)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

}
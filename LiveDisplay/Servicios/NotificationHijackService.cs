using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Service.Notification;
using Android.Graphics;
using System.IO;
using Android.Support.V4.Content;

namespace LiveDisplay.Servicios
{
    class NotificationHijackService : NotificationListenerService 
    {

        Context context;


        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            context = context.ApplicationContext;
            return StartCommandResult.Sticky;
        }
        public override void OnNotificationPosted(StatusBarNotification notification)
        {
            String nombrePaquete = notification.PackageName;
            String ticker = "";
            if (notification.Notification != null)
            {
                ticker = notification.Notification.TickerText.ToString();

            }
            Bundle extras = notification.Notification.Extras;
            String titulo = extras.GetString("android.title");
            String texto = extras.GetCharSequence("android.text").ToString();
            int id1 = extras.GetInt(Notification.ExtraSmallIcon);
            Bitmap id = notification.Notification.LargeIcon;

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
            LocalBroadcastManager.GetInstance(context).SendBroadcast(notifSender);

        }
        public override void OnNotificationRemoved(StatusBarNotification notification)
        {
            Console.WriteLine("notificacion removida");
        }

    }
}
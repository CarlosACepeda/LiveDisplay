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

namespace LiveDisplay.Servicios
{
    class NotificationHijackService : NotificationListenerService 
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {

            return StartCommandResult.Sticky;
        }
        public void OnNotificationPosted(StatusBarNotification notification)
        {
            String titulo = notification.PackageName;
            String ticker = "";
            if (notification.Notification != null)
            {
                ticker = notification.Notification.TickerText.ToString();

            }
        }
        
    }
}
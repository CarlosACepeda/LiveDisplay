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

namespace LiveDisplay.servicios
{
    [Service(Name = "undergrounddev.serv.BroadcastService", Label = "Broadcast Service")]
    class BroadcastService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            RegisterReceiver(new NotificationReceiver(), new IntentFilter("notificationSender"));
            return StartCommandResult.Sticky;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterReceiver(new NotificationReceiver());
        }
    }
}
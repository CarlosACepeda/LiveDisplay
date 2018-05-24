using Android.App;
using Android.Content;
using Android.OS;
using Android.Service.Notification;
using Android.Util;
using LiveDisplay.Adapters;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDisplay.Servicios
{
    [Service(Label = "Catcher", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE", Process =":NotificationCatcher")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService"})]
    internal class Catcher : NotificationListenerService
    {
        CatcherHelper catcherHelper;
        List<StatusBarNotification> statusBarNotifications;
        
        public override IBinder OnBind(Intent intent)
        {
            //Workaround for Kitkat to Retrieve Notifications.
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                RetrieveNotificationFromStatusBar();
                SubscribeToEvents();
                RegisterReceivers();
            }           
            return base.OnBind(intent);

        }
        public override void OnListenerConnected()
        {
            RetrieveNotificationFromStatusBar();
            SubscribeToEvents();
            RegisterReceivers();
            
        }
        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            catcherHelper.InsertNotification(sbn);
            Intent intent = new Intent();
            intent.SetAction("CatcherIntent");
            intent.AddFlags(ActivityFlags.ReceiverForeground);
            intent.PutExtra("Sample Text", "NotificationWasPosted");
            SendBroadcast(intent);
            base.OnNotificationPosted(sbn);
        }
        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            catcherHelper.RemoveNotification(sbn);
            base.OnNotificationRemoved(sbn);
        }
        public override void OnListenerDisconnected()
        {
            catcherHelper = null;
            base.OnListenerDisconnected();
        }
        public override bool OnUnbind(Intent intent)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                catcherHelper = null;
            }
                
            return base.OnUnbind(intent);
            
        }
        private void RetrieveNotificationFromStatusBar()
        {
            statusBarNotifications = new List<StatusBarNotification>();
            foreach (var notification in GetActiveNotifications().ToList())
            {
                if (notification.IsClearable == true)
                {
                    statusBarNotifications.Add(notification);
                }                
            }
            catcherHelper = new CatcherHelper(statusBarNotifications);
        }
        //Subscribe to events by NotificationSlave
        private void SubscribeToEvents()
        {
            NotificationSlave notificationSlave = new NotificationSlave();
            notificationSlave.AllNotificationsCancelled += NotificationSlave_AllNotificationsCancelled;
            notificationSlave.NotificationCancelled += NotificationSlave_NotificationCancelled;
            notificationSlave.NotificationCancelledLollipop += NotificationSlave_NotificationCancelledLollipop;
        }

        private void RegisterReceivers()
        {
            RegisterReceiver(new ScreenOnOffReceiver(), new IntentFilter(Intent.ActionScreenOn));
            RegisterReceiver(new ScreenOnOffReceiver(), new IntentFilter(Intent.ActionScreenOff));
        }

        //Events:
        private void NotificationSlave_NotificationCancelledLollipop(object sender, NotificationCancelledEventArgsLollipop e)
        {
            CancelNotification(e.Key);
        }

        private void NotificationSlave_NotificationCancelled(object sender, NotificationCancelledEventArgsKitkat e)
        {
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
            CancelNotification(e.NotificationPackage, e.NotificationTag, e.NotificationId);
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
        }

        private void NotificationSlave_AllNotificationsCancelled(object sender, EventArgs e)
        {
            CancelAllNotifications();
        }

    }
}
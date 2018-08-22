using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Service.Notification;
using Android.Util;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Servicios.Music;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDisplay.Servicios
{
    [Service(Label = "@string/app_name", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    internal class Catcher : NotificationListenerService
    {
        //Move to respective fragments
        private BatteryReceiver batteryReceiver;
        private ScreenOnOffReceiver screenOnOffReceiver;

        //Manipular las sesiones-
        private MediaSessionManager mediaSessionManager;
        //el controlador actual de media.
        private ActiveMediaSessionsListener activeMediaSessionsListener;

        //For Kitkat Music Controlling.

#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
        private RemoteController remoteController;
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
        
        private AudioManager audioManager;

        private CatcherHelper catcherHelper;
        private List<StatusBarNotification> statusBarNotifications;
#pragma warning disable CS0414 // El campo 'Catcher.isInAPlainSurface' está asignado pero su valor nunca se usa
        bool isInAPlainSurface = false;
#pragma warning restore CS0414 // El campo 'Catcher.isInAPlainSurface' está asignado pero su valor nunca se usa

        public override IBinder OnBind(Intent intent)
        {
            //Workaround for Kitkat to Retrieve Notifications.
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                //Activate the remote controller in Kitkat, because is Deprecated since Lollipop

                ThreadPool.QueueUserWorkItem(o =>
                {
                    Thread.Sleep(1000);
                    RetrieveNotificationFromStatusBar();
                });

                SubscribeToEvents();
                RegisterReceivers();
                //New remote controller for Kitkat

#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
                remoteController = new RemoteController(Application.Context, new MusicControllerKitkat());
                
                    //remoteController.SetArtworkConfiguration(450, 450);
                
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
                    audioManager= (AudioManager)Application.Context.GetSystemService(AudioService);
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
                    audioManager.RegisterRemoteController(remoteController);
              
                    
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
                
            }
            return base.OnBind(intent);
        }

        public override void OnListenerConnected()
        {
            activeMediaSessionsListener = new ActiveMediaSessionsListener();
            //RemoteController Lollipop and Beyond Implementation
            mediaSessionManager = (MediaSessionManager)GetSystemService(MediaSessionService);

            //Listener para Sesiones
            using (var h = new Handler(Looper.MainLooper)) //Using UI Thread because seems to crash in some devices.
                h.Post(() => { mediaSessionManager.AddOnActiveSessionsChangedListener(activeMediaSessionsListener, new ComponentName(this, Java.Lang.Class.FromType(typeof(Catcher))));
                    Log.Info("LiveDisplay", "Added Media Sess. Changed Listener");
                });
            


            SubscribeToEvents();
            RegisterReceivers();
            RetrieveNotificationFromStatusBar();
            //TODO:This setting is sensible to user configuration and also the Inactive hours setting.
            //Move to NotificationFragment.
            //StartWatchingDeviceMovement();
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            base.OnNotificationPosted(sbn);
            catcherHelper.OnNotificationPosted(sbn);
        }

        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            base.OnNotificationRemoved(sbn);
            catcherHelper.OnNotificationRemoved(sbn);
            
        }

        public override void OnListenerDisconnected()
        {
            catcherHelper.Dispose();
            mediaSessionManager.RemoveOnActiveSessionsChangedListener(activeMediaSessionsListener);
            UnregisterReceivers();
            base.OnListenerDisconnected();
        }

        public override bool OnUnbind(Intent intent)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                catcherHelper.Dispose();
                UnregisterReceivers();
                audioManager.UnregisterRemoteController(remoteController);
            }

            return base.OnUnbind(intent);
        }

        private void RetrieveNotificationFromStatusBar()
        {
            statusBarNotifications = new List<StatusBarNotification>();
            foreach (var notification in GetActiveNotifications().ToList())
            {
                if ((notification.IsOngoing==false||notification.Notification.Flags.HasFlag(NotificationFlags.NoClear))&&notification.IsClearable==true)
                {
                    statusBarNotifications.Add(notification);
                }
            }

            catcherHelper = new CatcherHelper(statusBarNotifications);
        }

        //Subscribe to events by NotificationSlave
        private void SubscribeToEvents()
        {
            NotificationSlave notificationSlave = NotificationSlave.NotificationSlaveInstance();
            notificationSlave.AllNotificationsCancelled += NotificationSlave_AllNotificationsCancelled;
            notificationSlave.NotificationCancelled += NotificationSlave_NotificationCancelled;
            notificationSlave.NotificationCancelledLollipop += NotificationSlave_NotificationCancelledLollipop;
        }
        private void RegisterReceivers()
        {
            using (IntentFilter intentFilter = new IntentFilter())
            {
                    screenOnOffReceiver = new ScreenOnOffReceiver();
                    intentFilter.AddAction(Intent.ActionScreenOff);
                    intentFilter.AddAction(Intent.ActionScreenOn);
                    RegisterReceiver(screenOnOffReceiver, intentFilter);
            }
            using (IntentFilter intentFilter = new IntentFilter())
            {
                batteryReceiver = new BatteryReceiver();
                intentFilter.AddAction(Intent.ActionBatteryChanged);
                RegisterReceiver(batteryReceiver, intentFilter);
            }
                        
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
            catcherHelper.CancelAllNotifications();
        }

        private void UnregisterReceivers()
        {
            UnregisterReceiver(screenOnOffReceiver);
            UnregisterReceiver(batteryReceiver);
        }
    }
}
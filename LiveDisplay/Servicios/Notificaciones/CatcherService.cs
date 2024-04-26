using Android.App;
using Android.Content;
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
    [Service(Label = "@string/app_name", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE", Exported = true)]
    [IntentFilter(new[] { ServiceInterface })]
    
    internal class Catcher : NotificationListenerService, RemoteController.IOnClientUpdateListener
    {
        private ScreenOnOffReceiver screenOnOffReceiver;
        private MediaSessionManager mediaSessionManager;
        private MediaEventsPublisherKitkat musicControllerKitkat;
        private ActiveMediaSessionsListener activeMediaSessionsListener;
        private RemoteController remoteController;
        private AudioManager audioManager;
        private CatcherHelper catcherHelper;
        private OpenNotification lastPostedNotification;

        public override void OnListenerHintsChanged([GeneratedEnum] NotificationListenerServiceHint hints)
        {
            Console.WriteLine($"Hints {hints}");
            base.OnListenerHintsChanged(hints);
        }
        public override IBinder OnBind(Intent intent)
        {
            //Workaround for Kitkat to Retrieve Notifications.
            if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Thread.Sleep(1000);
                    RetrieveNotificationFromStatusBar();
                });

                SubscribeToEvents();
                RegisterReceivers();
                remoteController = new RemoteController(Application.Context, this); //Could leak.
                remoteController.SetArtworkConfiguration(Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.HeightPixels);
                audioManager = (AudioManager)Application.Context.GetSystemService(AudioService);
                audioManager.RegisterRemoteController(remoteController);
                musicControllerKitkat = MediaEventsPublisherKitkat.Initialize(remoteController);
            }
            return base.OnBind(intent);
        }

        public override void OnListenerConnected()
        {
            ScreenOnOffReceiver.ReceiverCount++;
            activeMediaSessionsListener = new ActiveMediaSessionsListener();
            //RemoteController Lollipop and Beyond Implementation
            mediaSessionManager = (MediaSessionManager)GetSystemService(MediaSessionService);

            ////Listener para Sesiones
            //using (var h = new Handler(Looper.MainLooper)) //Using UI Thread because seems to crash in some devices.
            //    h.Post(() =>
            //    {
            //        try
            //        {
            //            mediaSessionManager.AddOnActiveSessionsChangedListener(activeMediaSessionsListener, new ComponentName(this, Java.Lang.Class.FromType(typeof(Catcher))));
            //            Log.Info("LiveDisplay", "Added Media Sess. Changed Listener");
            //        }
            //        catch
            //        {
            //            Log.Info("LiveDisplay", "Failed to register Media Session Callback");
            //        }
            //    });

            SubscribeToEvents();
            RegisterReceivers();
            RetrieveNotificationFromStatusBar();
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            var openNotification = new OpenNotification(sbn);
            lastPostedNotification = openNotification;
            catcherHelper.OnNotificationPosted(openNotification);
            //Console.WriteLine($" RECEIVER COUNT:{ ScreenOnOffReceiver.ReceiverCount}");
        }

        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            catcherHelper.OnNotificationRemoved(new OpenNotification(sbn));
        }

        public override void OnListenerDisconnected()
        {
            catcherHelper.Dispose();
            //mediaSessionManager.RemoveOnActiveSessionsChangedListener(activeMediaSessionsListener);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                UnregisterReceiver(screenOnOffReceiver);
                ScreenOnOffReceiver.ReceiverCount--;
            }
            base.OnListenerDisconnected();
        }

        public override bool OnUnbind(Intent intent)
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.N)
            {
                catcherHelper.Dispose();
                if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
                {
                    
                    UnregisterReceiver(screenOnOffReceiver);
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
                    audioManager.UnregisterRemoteController(remoteController);
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
                }
                mediaSessionManager.RemoveOnActiveSessionsChangedListener(activeMediaSessionsListener);
                UnregisterReceiver(screenOnOffReceiver);
                ScreenOnOffReceiver.ReceiverCount--;
            }

            return base.OnUnbind(intent);
        }

        private void RetrieveNotificationFromStatusBar()
        {
            List<OpenNotification> openNotifications = new List<OpenNotification>();
            foreach (var notification in GetActiveNotifications()?.ToList())
            {
                var openNotification = new OpenNotification(notification);
                openNotifications.Add(openNotification);
                lastPostedNotification = openNotification;
            }

            catcherHelper = new CatcherHelper(openNotifications);
        }

        //Subscribe to events by Several publishers
        private void SubscribeToEvents()
        {
            NotificationSlave notificationSlave = NotificationSlave.NotificationSlaveInstance();
            notificationSlave.AllNotificationsCancelled += NotificationSlave_AllNotificationsCancelled;
            notificationSlave.NotificationCancelled += NotificationSlave_NotificationCancelled;
            notificationSlave.NotificationCancelledLollipop += NotificationSlave_NotificationCancelledLollipop;
            notificationSlave.ResendLastNotificationRequested += NotificationSlave_ResendLastNotificationRequested;
            
        }

        private void NotificationSlave_ResendLastNotificationRequested(object sender, EventArgs e)
        {
            catcherHelper.OnNotificationPosted(lastPostedNotification);
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
        }

        //Events:
        private void NotificationSlave_NotificationCancelledLollipop(object sender, NotificationCancelledEventArgsLollipop e)
        {
            try
            {
                CancelNotification(e.Key);
            }
            catch (Java.Lang.SecurityException ex)
            {
                Log.Info("LiveDisplay", $"Fail to dismiss the notification, listener was not ready: {ex.Message}");
            }
        }

        private void NotificationSlave_NotificationCancelled(object sender, NotificationCancelledEventArgsKitkat e)
        {
            try
            {
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
                CancelNotification(e.NotificationPackage, e.NotificationTag, e.NotificationId);
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
            }
            catch (Java.Lang.SecurityException)
            {
                Log.Info("LiveDisplay", "Fail to dismiss the notification, listener was not ready");
            }
        }

        private void NotificationSlave_AllNotificationsCancelled(object sender, EventArgs e)
        {
            try
            {
                CancelAllNotifications();
                catcherHelper.CancelAllNotifications();
            }
            catch (Java.Lang.SecurityException)
            {
                Log.Info("LiveDisplay", "Fail to dismiss the notification, listener was not ready");
            }
        }

        public void OnClientChange(bool clearing)
        {
            Log.Info("LiveDisplay", "clearing: " + clearing);
            musicControllerKitkat = MediaEventsPublisherKitkat.Initialize(remoteController);
        }

        public void OnClientMetadataUpdate(RemoteController.MetadataEditor metadataEditor)
        {
            musicControllerKitkat.OnMetadataChanged(metadataEditor);
        }

        public void OnClientPlaybackStateUpdateSimple([GeneratedEnum] RemoteControlPlayState stateSimple)
        {
            musicControllerKitkat.OnPlaybackStateChanged(stateSimple);
        }

        public void OnClientPlaybackStateUpdate([GeneratedEnum] RemoteControlPlayState state, long stateChangeTimeMs, long currentPosMs, float speed)
        {
            musicControllerKitkat.OnPlaybackStateChanged(state);
        }

        public void OnClientTransportControlUpdate([GeneratedEnum] RemoteControlFlags transportControlFlags)
        {
            Log.Info("Livedisplay", "TransportControl update" + transportControlFlags);
        }
    }
}
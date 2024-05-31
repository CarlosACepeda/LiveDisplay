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
    [Service(Label = "@string/app_name", Permission = Android.Manifest.Permission.BindNotificationListenerService, Exported = true)]
    [IntentFilter(new[] { ServiceInterface })]

    internal class Catcher : NotificationListenerService, RemoteController.IOnClientUpdateListener
    {
        private RemoteController remoteController;
        private ScreenOnOffReceiver screenOnOffReceiver;
        private MediaSessionManager mediaSessionManager;
        private MediaEventsPublisherKitkat musicControllerKitkat;
        private ActiveMediaSessionsListener activeMediaSessionsListener;
        private AudioManager audioManager;
        private CatcherHelper catcherHelper;
        private OpenNotification lastPostedNotification;
        private NotificationSlave notificationSlave;

        public override void OnInterruptionFilterChanged([GeneratedEnum] InterruptionFilterType interruptionFilter)
        {
            catcherHelper.OnZenModeChanged(interruptionFilter != InterruptionFilterType.All);
            base.OnInterruptionFilterChanged(interruptionFilter);
        }
        public override IBinder OnBind(Intent intent)
        {
            //Workaround for Kitkat to Retrieve Notifications.
            if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
            {
                Console.WriteLine("ONBIND!");

                ThreadPool.QueueUserWorkItem(o =>
                {
                    Thread.Sleep(2000);
                    RetrieveNotificationFromStatusBar();

                    audioManager = (AudioManager)Application.Context.GetSystemService(AudioService);
                    remoteController = new RemoteController(Application.Context, this, MainLooper); //Could leak.
                    remoteController.SetArtworkConfiguration(Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.HeightPixels);
                    RemoteControlClient client = new RemoteControlClient(null, MainLooper);
                    var session= client.MediaSession;
                    audioManager.RegisterRemoteController(remoteController);
                    musicControllerKitkat = MediaEventsPublisherKitkat.Initialize(remoteController);
                    ToggleNotificationSlaveSubscription(true);
                    RegisterScreenOnOffReceiver();
                });

            }
            return base.OnBind(intent);
        }

        public override void OnListenerConnected()
        {
            ScreenOnOffReceiver.ReceiverCount++;
            activeMediaSessionsListener = new ActiveMediaSessionsListener();
            //RemoteController Lollipop and Beyond Implementation
            mediaSessionManager = (MediaSessionManager)GetSystemService(MediaSessionService);
            ToggleNotificationSlaveSubscription(true);
            RegisterScreenOnOffReceiver();
            RetrieveNotificationFromStatusBar();
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            var openNotification = new OpenNotification(sbn);
            lastPostedNotification = openNotification;
            catcherHelper.OnNotificationPosted(openNotification);
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
                ToggleNotificationSlaveSubscription(false);
                UnregisterReceiver(screenOnOffReceiver);
                ScreenOnOffReceiver.ReceiverCount--;
            }
            base.OnListenerDisconnected();
        }

        public override bool OnUnbind(Intent intent)
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.M)
            {
                catcherHelper?.Dispose();
                if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
                {
                    Console.WriteLine("ON UNBIND!");
                    if(remoteController!=null)
                        audioManager?.UnregisterRemoteController(remoteController);
                }
                else
                {
                    mediaSessionManager.RemoveOnActiveSessionsChangedListener(activeMediaSessionsListener);
                    UnregisterReceiver(screenOnOffReceiver);
                }

                ToggleNotificationSlaveSubscription(false);
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

        private void ToggleNotificationSlaveSubscription(bool subscribe)
        {
            notificationSlave = NotificationSlave.GetInstance();
            if (subscribe)
            {
                notificationSlave.AllNotificationsCancelled += NotificationSlave_AllNotificationsCancelled;
                notificationSlave.NotificationCancelled += NotificationSlave_NotificationCancelled;
                notificationSlave.NotificationCancelledLollipop += NotificationSlave_NotificationCancelledLollipop;
                notificationSlave.ResendLastNotificationRequested += NotificationSlave_ResendLastNotificationRequested;

            }
            else
            {
                notificationSlave.AllNotificationsCancelled -= NotificationSlave_AllNotificationsCancelled;
                notificationSlave.NotificationCancelled -= NotificationSlave_NotificationCancelled;
                notificationSlave.NotificationCancelledLollipop -= NotificationSlave_NotificationCancelledLollipop;
                notificationSlave.ResendLastNotificationRequested -= NotificationSlave_ResendLastNotificationRequested;
            }

        }

        private void NotificationSlave_ResendLastNotificationRequested(object sender, EventArgs e)
        {
            catcherHelper.OnNotificationPosted(lastPostedNotification);
        }

        private void RegisterScreenOnOffReceiver()
        {
            using IntentFilter intentFilter = new IntentFilter();
            screenOnOffReceiver = new ScreenOnOffReceiver();
            intentFilter.AddAction(Intent.ActionScreenOff);
            intentFilter.AddAction(Intent.ActionScreenOn);
            RegisterReceiver(screenOnOffReceiver, intentFilter);
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
                CancelNotification(e.NotificationPackage, e.NotificationTag, e.NotificationId);
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
            Log.Info("ONCLIENT CHANGE", "CLEARING: " + clearing);
        }

        public void OnClientMetadataUpdate(RemoteController.MetadataEditor metadataEditor)
        {
            musicControllerKitkat.OnMetadataChanged(metadataEditor);
        }

        public void OnClientPlaybackStateUpdateSimple([GeneratedEnum] RemoteControlPlayState stateSimple)
        {
            Console.WriteLine("client PlaybackState UPDATE SSIMPLE");
            musicControllerKitkat.OnPlaybackStateChanged(stateSimple);
        }

        public void OnClientPlaybackStateUpdate([GeneratedEnum] RemoteControlPlayState state, long stateChangeTimeMs, long currentPosMs, float speed)
        {
            Console.WriteLine("client PlaybackState UPDATE");
            musicControllerKitkat.OnPlaybackStateChanged(state);
        }

        public void OnClientTransportControlUpdate([GeneratedEnum] RemoteControlFlags transportControlFlags)
        {
            Log.Info("Livedisplay", "TransportControl update" + transportControlFlags);
        }
    }
}
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

#pragma warning disable CS0618//RemoteController obsolete, for Kitkat, omit this warning

    internal class Catcher : NotificationListenerService, RemoteController.IOnClientUpdateListener
    {
        private RemoteController remoteController;
#pragma warning restore CS0618

        private ScreenOnOffReceiver screenOnOffReceiver;
        private MediaSessionManager mediaSessionManager;
        private MediaEventsPublisherKitkat musicControllerKitkat;
        private ActiveMediaSessionsListener activeMediaSessionsListener;
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
                Console.WriteLine("ONBIND!");

                ThreadPool.QueueUserWorkItem(o =>
                {
                    Thread.Sleep(2000);
                    RetrieveNotificationFromStatusBar();

                    audioManager = (AudioManager)Application.Context.GetSystemService(AudioService);

#pragma warning disable CS0618//Obsolete for Kitkat, omit this warning
                    remoteController = new RemoteController(Application.Context, this, MainLooper); //Could leak.
                    remoteController.SetArtworkConfiguration(Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.HeightPixels);
                    audioManager.RegisterRemoteController(remoteController);
#pragma warning restore CS0618

                    musicControllerKitkat = MediaEventsPublisherKitkat.Initialize(remoteController);
                });

                SubscribeToEvents();
                RegisterReceivers();

            }
            return base.OnBind(intent);
        }

        public override void OnListenerConnected()
        {
            ScreenOnOffReceiver.ReceiverCount++;
            activeMediaSessionsListener = new ActiveMediaSessionsListener();
            //RemoteController Lollipop and Beyond Implementation
            mediaSessionManager = (MediaSessionManager)GetSystemService(MediaSessionService);
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
                catcherHelper?.Dispose();
                if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
                {
                    Console.WriteLine("ON UNBIND!");
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
                    if(remoteController!=null)
                        audioManager?.UnregisterRemoteController(remoteController);
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
                }
                else
                {
                    mediaSessionManager.RemoveOnActiveSessionsChangedListener(activeMediaSessionsListener);
                    UnregisterReceiver(screenOnOffReceiver);
                }
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
#pragma warning disable CS0618 //Cancel Notification for Kitkat
                CancelNotification(e.NotificationPackage, e.NotificationTag, e.NotificationId);
#pragma warning restore 
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

#pragma warning disable CS0618//RemoteController obsolete, for Kitkat, omit this warning

        public void OnClientChange(bool clearing)
        {
            Log.Info("ONCLIENT CHANGE", "CLEARING: " + clearing);
        }

        public void OnClientMetadataUpdate(RemoteController.MetadataEditor metadataEditor)
        {
#pragma warning restore

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
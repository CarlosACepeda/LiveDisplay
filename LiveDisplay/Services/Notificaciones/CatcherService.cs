using Android.App;
using Android.Content;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Service.Notification;
using Android.Util;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Services.Media;
using LiveDisplay.Services.Notifications;
using LiveDisplay.Services.Notifications.NotificationEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDisplay.Services
{
    [Service(Label = "@string/app_name", Permission = Android.Manifest.Permission.BindNotificationListenerService, Exported = true)]
    [IntentFilter(new[] { ServiceInterface })]

    internal class Catcher : NotificationListenerService, RemoteController.IOnClientUpdateListener
    {
        private RemoteController remoteController;
        private ScreenOnOffReceiver screenOnOffReceiver;
        private MediaSessionManager mediaSessionManager;
        private MediaEventsPublisherKitkat mediaControllerKitkat;
        private ActiveMediaSessionsListener activeMediaSessionsListener;
        private AudioManager audioManager;
        private CatcherHelper catcherHelper;
        private OpenNotification lastPostedNotification;
        private NotificationSlave notificationSlave;
        private const int MillisUntilSafeCallsKitkat= 2000;
        private bool everythingIsInitialized = false; //Android calls the Listener Connected/OnBind callbacks twice, this is to prevent double subscription.

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
                OnListenerConnectedKitkat();
            }
            return base.OnBind(intent);
        }

        public void OnListenerConnectedKitkat()
        {
            if (!everythingIsInitialized)
            {
                Console.WriteLine("ONBIND!");

                ThreadPool.QueueUserWorkItem(o =>
                {
                    Thread.Sleep(MillisUntilSafeCallsKitkat);
                    InitializeCatcherHelper(); //Must come first, subsequent calls depend on this initialization.

                    audioManager = (AudioManager)Application.Context.GetSystemService(AudioService);
                    remoteController = new RemoteController(Application.Context, this, MainLooper); //Could leak.
                    remoteController.SetArtworkConfiguration(Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.HeightPixels);
                    RemoteControlClient client = new RemoteControlClient(null, MainLooper);
                    audioManager.RegisterRemoteController(remoteController);
                    mediaControllerKitkat = MediaEventsPublisherKitkat.Initialize(remoteController);
                    ToggleNotificationSlaveSubscription(true);
                    RegisterScreenOnOffReceiver();
                    ToggleListeningForConfigurationChanges(true);
                    TryToInitializeMediaEventsListener();
                    ToggleInitializeVisualizerService(true);
                });
                everythingIsInitialized = true;
            }
        }


        public override void OnListenerConnected()
        {
            if (!everythingIsInitialized)
            {
                InitializeCatcherHelper(); //Must come first, subsequent calls depend on this initialization.
                ScreenOnOffReceiver.ReceiverCount++;
                //RemoteController Lollipop and Beyond Implementation
                ToggleNotificationSlaveSubscription(true);
                RegisterScreenOnOffReceiver();
                ToggleListeningForConfigurationChanges(true);
                TryToInitializeMediaEventsListener();
                ToggleInitializeVisualizerService(true);
                everythingIsInitialized = true;
            }
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            var openNotification = new OpenNotification(sbn);
            lastPostedNotification = openNotification;
            catcherHelper.OnNotificationPosted(openNotification);
            ToggleMediaEventsPublisherAvailability(openNotification, true);
        }

        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            var openNotification = new OpenNotification(sbn);
            catcherHelper.OnNotificationRemoved(openNotification);
            ToggleMediaEventsPublisherAvailability(openNotification, false);
        }

        public override void OnListenerDisconnected() //Nougat and beyond.
        {
            ToggleNotificationSlaveSubscription(false);
            ToggleListeningForConfigurationChanges(false);
            UnregisterReceiver(screenOnOffReceiver);
            FinalizeMediaEventsListener();
            ToggleInitializeVisualizerService(false);
            catcherHelper.Dispose();
            ScreenOnOffReceiver.ReceiverCount--;
            everythingIsInitialized = false;

            base.OnListenerDisconnected();
        }
        public void OnListenerDisconnectedMarshmallow()
        {
            ToggleNotificationSlaveSubscription(false);
            ToggleListeningForConfigurationChanges(false);
            UnregisterReceiver(screenOnOffReceiver);
            FinalizeMediaEventsListener();
            ToggleInitializeVisualizerService(false);
            catcherHelper.Dispose();
            ScreenOnOffReceiver.ReceiverCount--;
            everythingIsInitialized = false;
        }
        public void OnListenerDisconnectedKitkat()
        {
            if (remoteController != null)
                audioManager?.UnregisterRemoteController(remoteController);

            UnregisterReceiver(screenOnOffReceiver);
            ToggleNotificationSlaveSubscription(false);
            ToggleListeningForConfigurationChanges(false);
            ToggleInitializeVisualizerService(false);
            everythingIsInitialized = false;
        }

        public override bool OnUnbind(Intent intent)
        {
            Console.WriteLine("ON UNBIND!");
            if (Build.VERSION.SdkInt <= BuildVersionCodes.M && Build.VERSION.SdkInt>= BuildVersionCodes.Lollipop)
            {
                OnListenerDisconnectedMarshmallow();
            }
            else if(Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
            {
                OnListenerDisconnectedKitkat();
            }

            return base.OnUnbind(intent);
        }

        private List<OpenNotification> RetrieveNotificationFromStatusBar()
        {
            List<OpenNotification> openNotifications = new List<OpenNotification>();
            foreach (var notification in GetActiveNotifications()?.ToList())
            {
                var openNotification = new OpenNotification(notification);
                openNotifications.Add(openNotification);
                lastPostedNotification = openNotification;
            }
            return openNotifications;
        }
        void InitializeCatcherHelper()
        {
            if(catcherHelper== null)
            {
                var openNotifications = RetrieveNotificationFromStatusBar();
                catcherHelper = new CatcherHelper(openNotifications);
            }
        }
        void TryToInitializeMediaEventsListener()
        {
            foreach (var openNotification in RetrieveNotificationFromStatusBar())
            {
                ToggleMediaEventsPublisherAvailability(openNotification, true);
            }
        }
        void FinalizeMediaEventsListener()
        {
            if(MediaEventsPublisherLollipop.IsInitialized())
                MediaEventsPublisherLollipop.GetInstance().Finish();
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
                notificationSlave.RequestedOpenNotification += NotificationSlave_RequestedOpenNotification;
            }
            else
            {
                notificationSlave.AllNotificationsCancelled -= NotificationSlave_AllNotificationsCancelled;
                notificationSlave.NotificationCancelled -= NotificationSlave_NotificationCancelled;
                notificationSlave.NotificationCancelledLollipop -= NotificationSlave_NotificationCancelledLollipop;
                notificationSlave.ResendLastNotificationRequested -= NotificationSlave_ResendLastNotificationRequested;
                notificationSlave.RequestedOpenNotification -= NotificationSlave_RequestedOpenNotification;
            }

        }

        private void ToggleMediaEventsPublisherAvailability(OpenNotification openNotification, bool setAvailable)
        {
            var mediaSessionToken= openNotification.MediaSessionToken;
            var blockedSessions = RecentSessionsProvider.GetInstance().
                GetBlockedSessions();

            if (mediaSessionToken != null
                && (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                && !blockedSessions.Contains(openNotification.PackageName))
            {
                if (setAvailable)
                {
                    if (openNotification.Style == OpenNotification.MediaStyle)
                    {
                        if (openNotification.IsOngoing || !openNotification.IsAutoCancellable)
                        {
                            if (!MediaEventsPublisherLollipop.IsInitialized() || !MediaEventsPublisherLollipop.GetInstance().IsMediaSessionUsingToken(mediaSessionToken))
                            {
                                Console.WriteLine($"CATCHER: Trying initializing Media for: {openNotification.AppName}");
                                MediaEventsPublisherLollipop.Initialize(mediaSessionToken);
                            }
                        }
                    }
                }
                else
                {
                    if (MediaEventsPublisherLollipop.IsInitialized() &&
                        MediaEventsPublisherLollipop.GetInstance().IsMediaSessionUsingToken(mediaSessionToken))
                    {
                        MediaEventsPublisherLollipop.GetInstance().Finish();
                    }
                }
            }
        }

        private void NotificationSlave_RequestedOpenNotification(object sender, OpenNotificationRequestedEventArgs e)
        {
            catcherHelper.OnOpenNotificationRequested(e.Predicate, e.RequestCode);
        }

        private void ToggleListeningForConfigurationChanges(bool listening)
        {
            var serviceIntent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(SharedPreferenceListenerService)));
            if (listening)
                StartService(serviceIntent);
            else
                StopService(serviceIntent);
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

            var serviceIntent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(MediaControlsProviderService)));
            StartService(serviceIntent);
        }
        private void ToggleInitializeVisualizerService(bool start)
        {
            if (start)
            {
                VisualizerService.GetInstance().Start();
            }
            else VisualizerService.GetInstance().Stop();
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
            mediaControllerKitkat.OnMetadataChanged(metadataEditor);
        }

        public void OnClientPlaybackStateUpdateSimple([GeneratedEnum] RemoteControlPlayState stateSimple)
        {
            Console.WriteLine("client PlaybackState UPDATE SSIMPLE");
            mediaControllerKitkat.OnPlaybackStateChanged(stateSimple);
        }

        public void OnClientPlaybackStateUpdate([GeneratedEnum] RemoteControlPlayState state, long stateChangeTimeMs, long currentPosMs, float speed)
        {
            Console.WriteLine("client PlaybackState UPDATE");
            mediaControllerKitkat.OnPlaybackStateChanged(state);
        }

        public void OnClientTransportControlUpdate([GeneratedEnum] RemoteControlFlags transportControlFlags)
        {
            Log.Info("Livedisplay", "TransportControl update" + transportControlFlags);
            mediaControllerKitkat.OnTransportControlsUpdate(transportControlFlags);
        }
    }
}
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
    [Service(Label = "@string/app_name", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    internal class Catcher : NotificationListenerService, RemoteController.IOnClientUpdateListener
    {
        private ScreenOnOffReceiver screenOnOffReceiver;
        private MediaSessionManager mediaSessionManager;
        private MusicControllerKitkat musicControllerKitkat;
        private ActiveMediaSessionsListener activeMediaSessionsListener;
        private RemoteController remoteController;
        private AudioManager audioManager;
        private CatcherHelper catcherHelper;
        private List<StatusBarNotification> statusBarNotifications;

        public override IBinder OnBind(Intent intent)
        {
            //Workaround for Kitkat to Retrieve Notifications.
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Thread.Sleep(1000);
                    RetrieveNotificationFromStatusBar();
                });

                SubscribeToEvents();
                RegisterReceivers();
                remoteController = new RemoteController(Application.Context, this);
                remoteController.SetArtworkConfiguration(Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.HeightPixels);
                audioManager = (AudioManager)Application.Context.GetSystemService(AudioService);
                audioManager.RegisterRemoteController(remoteController);
                musicControllerKitkat = MusicControllerKitkat.GetInstance(remoteController);
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
                h.Post(() =>
                {
                    try
                    {
                        mediaSessionManager.AddOnActiveSessionsChangedListener(activeMediaSessionsListener, new ComponentName(this, Java.Lang.Class.FromType(typeof(Catcher))));
                        Log.Info("LiveDisplay", "Added Media Sess. Changed Listener");
                    }
                    catch
                    {
                        Log.Info("LiveDisplay", "Failed to register Media Session Callback");
                    }
                });

            SubscribeToEvents();
            RegisterReceivers();
            //This is for blocking Headsup notifications in Android Marshmallow and on, it does not work though LOL, stupid Android.
            //NotificationManager notificationManager = GetSystemService(NotificationService) as NotificationManager;
            //notificationManager.NotificationPolicy = new NotificationManager.Policy(NotificationPriorityCategory.Alarms | NotificationPriorityCategory.Calls | NotificationPriorityCategory.Events | NotificationPriorityCategory.Media | NotificationPriorityCategory.Messages | NotificationPriorityCategory.Reminders | NotificationPriorityCategory.RepeatCallers | NotificationPriorityCategory.System | NotificationPriorityCategory.RepeatCallers, NotificationPrioritySenders.Starred, NotificationPrioritySenders.Starred);
            //notificationManager.SetInterruptionFilter(InterruptionFilter.None);
            RetrieveNotificationFromStatusBar();
        }

        public override void OnNotificationPosted(StatusBarNotification sbn)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                //Let's attach the NotificationChannels that this package represents to this StatusbarNotification
                try
                {
                    var channels= GetNotificationChannels(sbn.PackageName, sbn.User);
                }
                catch (Exception ex)
                {
                    Log.Info("LiveDisplay", "Oops!: " + ex.Message);
                }
            }
            catcherHelper.OnNotificationPosted(sbn);

            //var test6 = sbn.Notification.Extras.Get(Notification.ExtraMediaSession) as MediaSession.Token;

            //if (test6 != null)
            //{
            //    try
            //    {
            //        MediaController mediaController = new MediaController(this, test6);

            //        var musicController = MusicController.GetInstance();
            //        mediaController.RegisterCallback(musicController);
            //        //Retrieve the controls to control the media, duh.
            //        musicController.TransportControls = mediaController.GetTransportControls();
            //        musicController.MediaMetadata = mediaController.Metadata;
            //        musicController.PlaybackState = mediaController.PlaybackState;
            //    }
            //    catch
            //    {
            //        //mediaController?.UnregisterCallback(musicController);
            //        //musicController.Dispose();
            //    }


            //    //mediaController.RegisterCallback(MusicController.GetInstance());
            //}
        }

        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            catcherHelper.OnNotificationRemoved(sbn);
        }

        public override void OnListenerDisconnected()
        {
            catcherHelper.Dispose();
            mediaSessionManager.RemoveOnActiveSessionsChangedListener(activeMediaSessionsListener);
            UnregisterReceiver(screenOnOffReceiver);
            base.OnListenerDisconnected();
        }

        public override bool OnUnbind(Intent intent)
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.N)
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    catcherHelper.Dispose();
                    UnregisterReceiver(screenOnOffReceiver);
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
                    audioManager.UnregisterRemoteController(remoteController);
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
                }
                else
                {
                    catcherHelper.Dispose();
                    mediaSessionManager.RemoveOnActiveSessionsChangedListener(activeMediaSessionsListener);
                    UnregisterReceiver(screenOnOffReceiver);
                }
            }

            return base.OnUnbind(intent);
        }

        

        private void RetrieveNotificationFromStatusBar()
        {
            statusBarNotifications = new List<StatusBarNotification>();
            foreach (var notification in GetActiveNotifications()?.ToList())
            {
                //var test6 = notification.Notification.Extras.Get(Notification.ExtraMediaSession) as MediaSession.Token;

                //if (test6 != null)
                //{
                //    MediaController mediaController = new MediaController(this, test6);

                //    mediaController.RegisterCallback(MusicController.GetInstance());
                //}

                //var test1 = notification.Notification.Extras.GetString(Notification.ExtraTemplate);
                //var test2 = notification.Notification.Extras;
                //var test3 = notification.Notification.Flags;
                //var test4 = notification.Notification.Extras.GetCharSequence(Notification.ExtraSummaryText);
                //var test5 = notification.Notification.Extras.GetCharSequenceArray(Notification.ExtraTextLines);
                //var test6 = notification.Notification.Extras.Get("android.wearable.EXTENSIONS");
                //var test7 = notification.Notification.Extras.KeySet();

                if ((notification.IsOngoing == false || notification.Notification.Flags.HasFlag(NotificationFlags.NoClear)) && notification.IsClearable == true)
                {
                    statusBarNotifications.Add(notification);
                   
                }
            }

            catcherHelper = new CatcherHelper(statusBarNotifications);
        }

        //Subscribe to events by Several publishers
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
        }

        //Events:
        private void NotificationSlave_NotificationCancelledLollipop(object sender, NotificationCancelledEventArgsLollipop e)
        {
            try
            {
                CancelNotification(e.Key);
            }
            catch (Java.Lang.SecurityException)
            {
                Log.Info("LiveDisplay", "Fail to dismiss the notification, listener was not ready");
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
            musicControllerKitkat = MusicControllerKitkat.GetInstance(remoteController);
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
﻿using Android.App;
using Android.Content;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Service.Notification;
using Android.Util;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Models;
using LiveDisplay.Services.Music;
using LiveDisplay.Services.Notifications;
using LiveDisplay.Services.Notifications.NotificationEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDisplay.Services
{
    [Service(Label = "@string/notification_listener", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    internal class NotificationHijackerService : NotificationListenerService, RemoteController.IOnClientUpdateListener
    {
        private ScreenOnOffReceiver screenOnOffReceiver;
        private MediaSessionManager mediaSessionManager;
        private MusicControllerKitkat musicControllerKitkat;
        private ActiveMediaSessionsListener activeMediaSessionsListener;
        private RemoteController remoteController;
        private AudioManager audioManager;
        private NotificationHijackerWorker notificationHijackerWorker;
        private List<StatusBarNotification> statusBarNotifications;
        private StatusBarNotification lastPostedNotification;

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
                InitializeRemoteController();
            }
            return base.OnBind(intent);
        }

        public override void OnListenerConnected()
        {
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
            lastPostedNotification = sbn;
            notificationHijackerWorker.OnNotificationPosted(new OpenNotification(sbn));

            //var test1 = sbn.Notification.Extras.GetString(Notification.ExtraTemplate);
            //var test2 = sbn.Notification.Extras;
            //var test3 = sbn.Notification.Flags;
            //var test4 = sbn.Notification.Extras.GetCharSequence(Notification.ExtraSummaryText);
            //var test5 = sbn.Notification.Extras.GetCharSequenceArray(Notification.ExtraTextLines);
            //var test6 = sbn.Notification.Extras.Get("android.wearable.EXTENSIONS");
            //var test7 = sbn.Notification.Extras.KeySet();
            //var test8 = sbn.Notification.Extras.Get("android.people.list");
            //var test10 = sbn.Notification.Extras.Get("android.messagingUser");
            //var test11 = sbn.Notification.Extras.Get("android.messagingStyleUser");
            //var test12 = sbn.Notification.Extras.Get("android.messages");
            //var test13 = sbn.Notification.Extras.GetParcelableArray("android.messages");
            //var test15 = (NotificationPriority)sbn.Notification.Priority;
            //foreach (Bundle item in test13)
            //{
            //    var test14 = item.KeySet();
            //    var moreExtras = item.Get("extras");
            //    var sender_person = item.Get("sender_person");
            //    var sender = item.Get("sender");
            //    var text = item.Get("text");
            //    var time = item.Get("time");
            //}

        }

        public override void OnNotificationRemoved(StatusBarNotification sbn)
        {
            notificationHijackerWorker.OnNotificationRemoved(new OpenNotification(sbn));
        }

        public override void OnListenerDisconnected()
        {
            notificationHijackerWorker.Dispose();
            mediaSessionManager?.RemoveOnActiveSessionsChangedListener(activeMediaSessionsListener);
            UnregisterReceiver(screenOnOffReceiver);
            base.OnListenerDisconnected();
        }

        public override bool OnUnbind(Intent intent)
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.N)
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    notificationHijackerWorker.Dispose();
                    UnregisterReceiver(screenOnOffReceiver);
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
                    audioManager.UnregisterRemoteController(remoteController);
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
                }
                else
                {
                    notificationHijackerWorker.Dispose();
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
                //var test8 = notification.Notification.Extras.Get("android.people.list");
                //var test10= notification.Notification.Extras.Get("android.messagingUser");
                //var test11= notification.Notification.Extras.Get("android.messagingStyleUser");
                //var test12= notification.Notification.Extras.Get("android.messages");
                //var test13 = notification.Notification.Extras.GetParcelableArray("android.messages");
                //if(test13 != null)
                //foreach (Bundle item in test13)
                //{
                //    var test14 = item.KeySet();
                //    var moreExtras = item.Get("extras");
                //    var sender_person = item.Get("sender_person");
                //    var sender = item.Get("sender");
                //    var text = item.Get("text");
                //    var time = item.Get("time");
                //        var uri = item.Get("uri");
                //        var type = item.Get("type");
                //}
                statusBarNotifications.Add(notification);
                lastPostedNotification = notification;
            }
            notificationHijackerWorker = NotificationHijackerWorker.GetInstance(statusBarNotifications, true); //Always recreate when listener is attached.
        }

        //Subscribe to events by Several publishers
        private void SubscribeToEvents()
        {
            NotificationSlave notificationSlave = NotificationSlave.NotificationSlaveInstance();
            notificationSlave.AllNotificationsCancelled += NotificationSlave_AllNotificationsCancelled;
            notificationSlave.NotificationCancelled += NotificationSlave_NotificationCancelled;
            notificationSlave.NotificationCancelledLollipop += NotificationSlave_NotificationCancelledLollipop;
            notificationSlave.ResendLastNotificationRequested += NotificationSlave_ResendLastNotificationRequested;
            notificationSlave.OnRequestedToggleMediaSessionsListener += NotificationSlave_OnRequestedToggleMediaSessionsListener;
        }

        private void NotificationSlave_OnRequestedToggleMediaSessionsListener(object sender, bool enable)
        {
            if(enable)
            {
                activeMediaSessionsListener = new ActiveMediaSessionsListener();
                //RemoteController Lollipop and Beyond Implementation
                mediaSessionManager = (MediaSessionManager)GetSystemService(MediaSessionService);

                using (var h = new Handler(Looper.MainLooper)) //Using UI Thread because seems to crash in some devices when not.
                    h.Post(() =>
                    {
                        try
                        {
                            mediaSessionManager.AddOnActiveSessionsChangedListener(activeMediaSessionsListener, new ComponentName(this, Java.Lang.Class.FromType(typeof(NotificationHijackerService))));
                            Log.Info("LiveDisplay", "Added Media Sess. Changed Listener");
                        }
                        catch
                        {
                            Log.Info("LiveDisplay", "Failed to register Media Session Callback");
                        }
                    });
            }
            else
            {
                mediaSessionManager?.RemoveOnActiveSessionsChangedListener(activeMediaSessionsListener);
                Log.Info("LiveDisplay", "REMOVED MediaSessions CHanged");
            }
        }

        private void NotificationSlave_ResendLastNotificationRequested(object sender, EventArgs e)
        {
            OnNotificationPosted(lastPostedNotification);
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
                notificationHijackerWorker.CancelAllNotifications();
            }
            catch (Java.Lang.SecurityException)
            {
                Log.Info("LiveDisplay", "Fail to dismiss the notification, listener was not ready");
            }
        }

        void InitializeRemoteController()
        {
            remoteController = new RemoteController(Application.Context, this);
            remoteController.SetArtworkConfiguration(Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.HeightPixels);
            audioManager = (AudioManager)Application.Context.GetSystemService(AudioService);
            audioManager.RegisterRemoteController(remoteController);
            musicControllerKitkat = MusicControllerKitkat.GetInstance(remoteController);
        }


        public void OnClientChange(bool clearing)
        {
            Log.Info("LiveDisplay", "clearing: " + clearing);
            if(clearing) musicControllerKitkat = MusicControllerKitkat.GetInstance(remoteController);
        }

        public void OnClientMetadataUpdate(RemoteController.MetadataEditor metadataEditor)
        {
            musicControllerKitkat.OnMetadataChanged(metadataEditor);
        }

        public void OnClientPlaybackStateUpdateSimple([GeneratedEnum] RemoteControlPlayState stateSimple)
        {
            //musicControllerKitkat.OnPlaybackStateChanged(stateSimple);
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
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Misc;
using LiveDisplay.Services.Media;
using LiveDisplay.Services.Media.MediaEventArgs;
using System;


namespace LiveDisplay.Services
{
    [Service(Label = "@string/app_name")]

    public class MediaControlsProviderService : Service
    {
        public const string MediaControlsProviderBroadcastServiceIntent = "MEDIA_CONTROLS";
        const int MediaControlsProviderServiceNotificationId = 1;
        const string MediaControlsProviderServiceNotificationChannelId = "MEDIA_CONTROLS_PROVIDER_SERVICE_CHANNEL";

        public const string ActionStopCommand = "STOP_COMMAND";
        public const string ActionCycleRepeatOptionCommand = "CYCLE_REPEAT_OPTION_COMMAND";

        string mediaTitle, mediaOwningApp, mediaArtist;
        PendingIntent mediaSessionPendingIntent;
        int repeatOptionSet;

        bool showStopControl, showRepeatControl = false;
        PendingIntent pendingIntentToStartOnClick;
        bool startLiveDisplayPlayer;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
            {
                MediaEventsPublisherKitkat.MediaMetadataChanged += MediaController_MediaMetadataChanged;
                MediaEventsPublisherKitkat.MediaRepeatOptionChanged += MediaEventsPublisher_MediaRepeatOptionChanged;
                MediaEventsPublisherKitkat.ControlsAvailabilityChanged += MediaEventsPublisher_ControlsAvailabilityChanged;
            }
            else
            {
                MediaEventsPublisherLollipop.MediaMetadataChanged += MediaController_MediaMetadataChanged;
                MediaEventsPublisherLollipop.MediaRepeatOptionChanged += MediaEventsPublisher_MediaRepeatOptionChanged;
                MediaEventsPublisherLollipop.ControlsAvailabilityChanged += MediaEventsPublisher_ControlsAvailabilityChanged;
                MediaEventsPublisherLollipop.PublisherFinished += MediaEventsPublisherLollipop_PublisherFinished;
            }
            
            SharedPreferenceListenerService.ConfigurationChanged += SharedPreferenceListenerService_ConfigurationChanged;
            ConfigurationManager configurationManager = new ConfigurationManager();
            SetPendingIntentActionForNotification(configurationManager.RetrieveAValue(ConfigurationParameters.MediaControlsProviderServiceNotificationActionIsExtAppPlayer));

            return base.OnStartCommand(intent, flags, startId);
        }

        private void SharedPreferenceListenerService_ConfigurationChanged(object sender, Configuration.ConfigurationChangedEventArgs e)
        {
            if(e.Key== ConfigurationParameters.MediaControlsProviderServiceNotificationActionIsExtAppPlayer)
            {
                SetPendingIntentActionForNotification((bool)e.Value);
            }
        }

        void SetPendingIntentActionForNotification(bool startsExternalPlayer)
        {
            if(startsExternalPlayer && mediaSessionPendingIntent!= null)
            {
                pendingIntentToStartOnClick = PendingIntent.GetActivity(Application.Context, 0, PackageUtils.GetAppIntent(mediaSessionPendingIntent.CreatorPackage), PendingIntentFlags.Immutable);
            }
            else pendingIntentToStartOnClick = PendingIntent.GetActivity(Application.Context, 0, new Intent(Application.Context, Java.Lang.Class.FromType(typeof(LockScreenActivity))), PendingIntentFlags.Immutable);
            UpdateNotification();
        }

        private void MediaEventsPublisher_ControlsAvailabilityChanged(object sender, ControlsAvailabilityChangedEventArgs e)
        {
            showStopControl = e.AvailableControls.HasFlag(Media.Enums.AvailableControls.Stop);
            showRepeatControl = e.AvailableControls.HasFlag(Media.Enums.AvailableControls.Repeat);
        }

        private void MediaEventsPublisherLollipop_PublisherFinished(object sender, bool e)
        {
            NotificationSlave.GetInstance().CancelOwnNotification(MediaControlsProviderServiceNotificationId);
        }

        private void MediaEventsPublisher_MediaRepeatOptionChanged(object sender, int e)
        {
            repeatOptionSet = e;
            UpdateNotification();
        }

        private void MediaController_MediaMetadataChanged(object sender, MediaMetadataChangedEventArgs e)
        {
            mediaOwningApp = e.AppName;
            mediaTitle = e.MediaTitle;
            mediaArtist = e.MediaArtist;
            mediaSessionPendingIntent = e.ActivityIntent;

            UpdateNotification();
        }

        private void UpdateNotification()
        {
            bool isOreo = Build.VERSION.SdkInt >= BuildVersionCodes.O;

            NotificationChannel notificationChannel = new NotificationChannel(MediaControlsProviderServiceNotificationChannelId, "LiveDisplay", NotificationImportance.High);

            NotificationManager notificationManager = GetSystemService(Service.NotificationService) as NotificationManager;

            notificationManager.CreateNotificationChannel(notificationChannel);


            var builder = isOreo ? new Notification.Builder(BaseContext, MediaControlsProviderServiceNotificationChannelId) : new Notification.Builder(BaseContext);
            builder.SetContentTitle($"{ GetString(Resource.String.extended_controls_for)} {mediaOwningApp}");
            builder.SetSubText(mediaTitle + " | " + mediaArtist);
            builder.SetSmallIcon(Resource.Drawable.ic_stat_default_appicon);
            builder.SetOnlyAlertOnce(true);

            Notification.Action[] actions= new Notification.Action[2];
            if(showRepeatControl)
            {
                var repeatIntent = new Intent(BaseContext, typeof(MediaControlsProviderBroadcastReceiver));
                repeatIntent.SetAction(ActionCycleRepeatOptionCommand);
                var repeatAction = new Notification.Action(GetRepeatOptionDrawableInt(repeatOptionSet), GetRepeatOptionText(), PendingIntent.GetBroadcast(BaseContext, 0, repeatIntent, PendingIntentFlags.Immutable));
                actions[0] = repeatAction;

            }
            if (showStopControl)
            {
                var stopIntent = new Intent(BaseContext, typeof(MediaControlsProviderBroadcastReceiver));
                stopIntent.SetAction(ActionStopCommand);
                var stopAction = new Notification.Action(Resource.Drawable.baseline_stop_white_24, GetString(Resource.String.stop), PendingIntent.GetBroadcast(BaseContext, 0, stopIntent, PendingIntentFlags.Immutable));
                actions[1] = stopAction;
            }

            builder.SetActions(actions);
            builder.SetContentIntent(pendingIntentToStartOnClick);
            builder.SetAutoCancel(false);
            NotificationSlave.GetInstance().PostNotification(
                MediaControlsProviderServiceNotificationId, builder);
        }

        private string GetRepeatOptionText()
        {
            switch (repeatOptionSet)
            {
                case IMediaEventsPublisher.DontRepeat:
                    return GetString(Resource.String.not_repeating);
                case IMediaEventsPublisher.RepeatOnce:
                    return GetString(Resource.String.repeating_once);
                case IMediaEventsPublisher.RepeatForever:
                    return GetString(Resource.String.always_repeating);
                default:
                    return string.Empty;
            }
        }

        int GetRepeatOptionDrawableInt(int repeatOption)
        {
            return repeatOption switch
            {
                IMediaEventsPublisher.DontRepeat => Resource.Drawable.outline_repeat_white_24,
                IMediaEventsPublisher.RepeatOnce => Resource.Drawable.outline_repeat_one_white_24,
                IMediaEventsPublisher.RepeatForever => Resource.Drawable.outline_repeat_on_white_24,
                _ => Resource.Drawable.outline_repeat_white_24,
            };
        }
        public override void OnDestroy()
        {
            NotificationSlave.GetInstance().CancelOwnNotification(MediaControlsProviderServiceNotificationId);
            base.OnDestroy();
        }
    }
}
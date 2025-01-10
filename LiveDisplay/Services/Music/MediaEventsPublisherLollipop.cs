using Android.App;
using Android.Graphics;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Util;
using Android.Views;
using LiveDisplay.Misc;
using LiveDisplay.Services.Media.Enums;
using LiveDisplay.Services.Media.MediaEventArgs;
using LiveDisplay.Services.Notifications;
using LiveDisplay.Services.Notifications.NotificationEventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Android.Media.Session.PlaybackState;

namespace LiveDisplay.Services.Media
{
    /// <summary>
    /// Use this class to receive Media Session updates by providing a MediaSession.Token
    /// </summary>
    internal class MediaEventsPublisherLollipop : MediaController.Callback, IMediaEventsPublisher
    {
        PlaybackState _playbackState;
        MediaController.TransportControls _transportControls;
        MediaMetadata _mediaMetadata;
        PendingIntent _activityIntent;
        MediaController _mediaController;
        MediaSession.Token _token;
        string _appname;
        MediaControlsBase _controls;
        static MediaEventsPublisherLollipop instance;
        const int MillisToRepeat = 2500;
        const int OneSecondInMillis = 1000;
        long currentProgress = 0;
        long totalProgress = 0;
        bool playbackStateChangedOnlyToChangeNotificationData = false;
        OpenNotification openNotification;
        private AvailableControls currentAvailableControls;

        const string ExtrasKeySlotReservationSeekToPrev = "android.media.playback.ALWAYS_RESERVE_SPACE_FOR.ACTION_SKIP_TO_PREVIOUS";
        const string ExtrasKeySlotReservationSeekToNext = "android.media.playback.ALWAYS_RESERVE_SPACE_FOR.ACTION_SKIP_TO_NEXT";
        const int MediaEventsOpenNotificationRequestCode = 1;


        bool resendingPlaybackState, resendingMediaMetadata, resendingRepeatOptionSet, resendingAvailableControls = false;


        System.Timers.Timer progressTimer = new System.Timers.Timer();

        private int repeatOptionSet;

        public static event EventHandler<MediaPlaybackStateChangedEventArgs> MediaPlaybackChanged
        {
            add
            {
                _mediaPlaybackChanged += value;
                //Welcome event:
                ResendSessionInformation(true, false, false, false); //Whoever subscribes to the _mediaPlaybackChanged event will get the latest PlaybackState Available, so we get the most recent data for the subscriber
            }
            remove
            {
                _mediaPlaybackChanged -= value;
            }
        }

        private static EventHandler<MediaPlaybackStateChangedEventArgs> _mediaPlaybackChanged;

        public static event EventHandler<MediaMetadataChangedEventArgs> MediaMetadataChanged
        {
            add
            {
                _mediaMetadataChanged += value;
                //Welcome event:
                ResendSessionInformation(false, true, true, false); //Whoever subscribes to the _mediaMetadataChanged event will get the latest MediaMetadata Available, so we get the most recent data for the subscriber
            }
            remove
            {
                _mediaMetadataChanged -= value;
            }
        }
        private static EventHandler<MediaMetadataChangedEventArgs> _mediaMetadataChanged;
        

        public static event EventHandler<MediaProgressChangedEventArgs> MediaProgressChanged;

        public static event EventHandler<int> MediaRepeatOptionChanged;

        public static event EventHandler<bool> PublisherFinished;

        public static event EventHandler<ControlsAvailabilityChangedEventArgs> ControlsAvailabilityChanged
        {
            add
            {
                _controlsAvailabilityChanged += value;
                //Welcome event:
                ResendSessionInformation(false, false, false, true); 
            }
            remove
            {
                _controlsAvailabilityChanged -= value;
            }
        }
        private static EventHandler<ControlsAvailabilityChangedEventArgs> _controlsAvailabilityChanged;


        private static void Initialize(MediaController controller)
        {
            //TODO: If finished by the user (Finish()) , don't let Catcher to start it again.

            if (instance == null)
                instance = new MediaEventsPublisherLollipop(controller);
            else if (controller.SessionToken.ToString() == instance._token.ToString())
            {
                //it's initialized, so lets send all the Media Session information.
                ResendSessionInformation(true, true, true, true);
                Log.Warn("LiveDisplay", "RESENDING DATA, ALREADY INIT");

            }
            else
            {
                //a new MediaSession requests to be initialized,
                instance.Finish(); //Finishing old
                instance = new MediaEventsPublisherLollipop(controller);
                Console.WriteLine("SUCCESS new MediaSession request, Switching...");
                Log.Warn("LiveDisplay", "SWITCHING");

            }
        }
        public static void Initialize(MediaSession.Token token)
        {
            if (token == null) throw new ArgumentNullException("Token can't be null!!");
            Initialize(new MediaController(Application.Context, token));
        }
        private MediaEventsPublisherLollipop(MediaController controller)
        {
            _mediaController = controller;
            _mediaController.RegisterCallback(this);
            _token = _mediaController.SessionToken;
            LoadMediaControllerData(_mediaController);
            _controls = MediaControlsBase.Instance;
            _controls.MediaEvent += MediaEvent;
            CatcherHelper.RequestedOpenNotificationResultGenerated += CatcherHelper_RequestedOpenNotificationResultGenerated;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            NotificationSlave.GetInstance().RequestOpenNotification(o => o.MediaSessionToken?.ToString() == _token.ToString(), MediaEventsOpenNotificationRequestCode);
            //Initialize the Media Notification.

            progressTimer.Interval = OneSecondInMillis;
            progressTimer.Elapsed += OnProgressTimerElapsed;
            RecentSessionsProvider.GetInstance().SaveSession(_mediaController.PackageName);
            Log.Warn("LiveDisplay", "CTOR SUCCESS");
        }

        private void CatcherHelper_NotificationPosted(object sender, Notifications.NotificationEventArgs.NotificationPostedEventArgs e)
        {
            if (e.OpenNotification.MediaSessionToken?.ToString() == _token.ToString())
            {
                openNotification = e.OpenNotification;
                playbackStateChangedOnlyToChangeNotificationData = true;
                OnPlaybackStateChanged(_playbackState);
            }
        }

        private void CatcherHelper_RequestedOpenNotificationResultGenerated(object sender, RequestedOpenNotificationGeneratedEventArgs e)
        {
            if (e.RequestCode == MediaEventsOpenNotificationRequestCode)
            {
                openNotification = e.OpenNotifications.FirstOrDefault(); 
            }
        }

        public static MediaEventsPublisherLollipop GetInstance()
        {
            if (instance == null) throw new InvalidOperationException("Call Initialize or InitializeFromToken First");
            return instance;
        }
        private void LoadMediaControllerData(MediaController controller)
        {
            if (controller != null)
            {
                _transportControls = controller.GetTransportControls();
                _appname = PackageUtils.GetTheAppName(controller.PackageName);
                //Invoke MediaMetadata, MediaPlayback, RepeatOption changed events, so all listeners will get notified of
                //the new Loaded mediacontroller.
                OnMetadataChanged(controller.Metadata);
                OnPlaybackStateChanged(controller.PlaybackState);
                OnMediaRepeatOptionChanged(repeatOptionSet);
            }
            else
            {
                throw new InvalidOperationException("How's this even possible?");
            }
        }
        public bool IsMediaSessionUsingToken(MediaSession.Token tokenToCheck)
        {
            return _mediaController.SessionToken.ToString() == tokenToCheck?.ToString();
        }
        public static bool IsInitialized()
        {
            return instance != null;
        }
        public bool IsActive()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                return _playbackState.IsActive;
            }
            else
            {
                return _playbackState.State == PlaybackStateCode.Buffering ||
                    _playbackState.State == PlaybackStateCode.Connecting ||
                    _playbackState.State == PlaybackStateCode.FastForwarding ||
                    _playbackState.State == PlaybackStateCode.Playing ||
                    _playbackState.State == PlaybackStateCode.Rewinding ||
                    _playbackState.State == PlaybackStateCode.SkippingToNext ||
                    _playbackState.State == PlaybackStateCode.SkippingToPrevious ||
                    _playbackState.State == PlaybackStateCode.SkippingToQueueItem;

            }
        }
        private void MediaEvent(object sender, MediaActionEventArgs e)
        {
            switch (e.MediaActionFlags)
            {
                case MediaActionFlags.Play:

                    _transportControls?.Play();
                    break;

                case MediaActionFlags.Pause:
                    _transportControls?.Pause();
                    break;

                case MediaActionFlags.SkipToNext:
                    _transportControls?.SkipToNext();
                    break;

                case MediaActionFlags.SkipToPrevious:
                    _transportControls?.SkipToPrevious();
                    break;

                case MediaActionFlags.SeekTo:
                    if (GetLongValue(MediaMetadata.MetadataKeyDuration) > 0) //in Live streams this value is 0. so we use it to prevent unwanted seek.
                        _transportControls?.SeekTo(e.Time);
                    break;

                case MediaActionFlags.FastFoward:
                    if (GetLongValue(MediaMetadata.MetadataKeyDuration) > 0)
                        _transportControls?.FastForward();
                    break;

                case MediaActionFlags.Rewind:
                    if (GetLongValue(MediaMetadata.MetadataKeyDuration) > 0)
                        _transportControls?.Rewind();
                    break;

                case MediaActionFlags.Stop:
                    _transportControls?.Stop();
                    break;

                case MediaActionFlags.RetrieveMediaInformation:

                    break;
                case MediaActionFlags.CycleRepeatOption:
                    CycleRepeatOption();
                    OnMediaRepeatOptionChanged(repeatOptionSet);
                    break;
                case MediaActionFlags.OpenRelatedActivity:
                    OpenRelatedActivity();
                    break;
                case MediaActionFlags.SendCustomAction:
                    _transportControls.SendCustomAction(e.CustomAction, e.CustomAction.Extras);
                    break;
                case MediaActionFlags.SendCompactedAction:
                    e.CompactedAction.ActionIntent?.Send();
                    break;
                default:
                    break;
            }
        }

        private void OpenRelatedActivity()
        {
            //The _activityIntent we get apparently sometimes causes a "CancelledException" when being sent
            //for that case we resort to send the Notification Pending Intent also
            KeyguardPendingIntentMediator.GetInstance().SendPendingIntent(_activityIntent,openNotification.ContentIntent);

        }

        public override void OnPlaybackStateChanged(PlaybackState state)
        {
            Console.WriteLine("OnPlaybackStateChanged CALLED");

            _playbackState = state;

            //The OnPlaybackStateChanged gets invoked by Android with an only updated PlaybackState,
            //including the current position of the media at the time of the call
            //But I also invoke it when the associated Notification gets some kind of update, 
            //so I pass the already held PlaybackState from a previous call, which can be outdated.
            //if this check is not performed then we'll inform the listeners that the current position of the media is that of the
            //old PlaybackState, which might be not the current position the Media is in, causing the Repeat feature to not work for example.

            if (playbackStateChangedOnlyToChangeNotificationData)
            {
                playbackStateChangedOnlyToChangeNotificationData = false;
                return;
            }
            //Only track progress when this call wasn't made by the Notification Posted event.
            //On when we arent re-sending media info, cuz this interferes with current progress management.
            if (!resendingPlaybackState)
                TrackProgress(state.Position, state.PlaybackSpeed);
            else resendingPlaybackState = false;


            OnMediaPlaybackChanged(new MediaPlaybackStateChangedEventArgs
            {
                PlaybackState = state.State,
                CurrentTime = state.Position,
                RepeatOptionSet = repeatOptionSet
            });
            OnControlsAvailabilityChanged(new ControlsAvailabilityChangedEventArgs
            {
                AvailableControls = SetAvailableControls(GetSupportedActions()),
                CustomActions= GetCustomActions(),
                TakeCustomActionsFromNotification= false, //TODO, ALLOW set.
                OpenNotification= openNotification
            });

            base.OnPlaybackStateChanged(state);

        }
        bool FindIfMediaSessionWantsToHideDefaultButtons(string extraKeySlotReservation)
        {
            if(_playbackState.Extras!= null)
            foreach (var key in _playbackState.Extras.KeySet())
            {
                if (key == extraKeySlotReservation)
                {
                    Console.WriteLine($"MEDIA SESSION WANTS TO HIDE DEFAULT BUTTON: {key}");
                    return _playbackState.Extras.GetBoolean(key);
                }
            }
            return false;
        }

        private List<CustomAction> GetCustomActions()
        {
            //Including Custom Actions
            return _playbackState.CustomActions.ToList();
        }

        public MediaSessionSupportedActionsFlags GetSupportedActions()
        {
            var supportedFlags = MediaSessionSupportedActionsFlags.None;

            Dictionary<long, string> actions = new Dictionary<long, string>
            {
                { PlaybackState.ActionStop, "ActionStop" },
                { PlaybackState.ActionPause, "ActionPause" },
                { PlaybackState.ActionPlay, "ActionPlay" },
                { PlaybackState.ActionRewind, "ActionRewind" },
                { PlaybackState.ActionSkipToPrevious, "ActionSkipToPrevious" },
                { PlaybackState.ActionSkipToNext, "ActionSkipToNext" },
                { PlaybackState.ActionFastForward, "ActionFastForward" },
                { PlaybackState.ActionSetRating, "ActionSetRating" },
                { PlaybackState.ActionSeekTo, "ActionSeekTo" },
                { PlaybackState.ActionPlayPause, "ActionPlayPause" },
                { PlaybackState.ActionPlayFromMediaId, "ActionPlayFromMediaId" },
                { PlaybackState.ActionPlayFromSearch, "ActionPlayFromSearch" },
                { PlaybackState.ActionSkipToQueueItem, "ActionSkipToQueueItem" },
                { PlaybackState.ActionPlayFromUri, "ActionPlayFromUri" },
                { PlaybackState.ActionPrepare, "ActionPrepare" },
                { PlaybackState.ActionPrepareFromMediaId, "ActionPrepareFromMediaId" },
                { PlaybackState.ActionPrepareFromSearch, "ActionPrepareFromSearch" },
                { PlaybackState.ActionPrepareFromUri, "ActionPrepareFromUri" },
                { PlaybackState.ActionSetPlaybackSpeed, "ActionSetPlaybackSpeed" },
            };

            foreach (var action in actions)
            {
                if ((_playbackState.Actions & action.Key) == action.Key)
                {
                    supportedFlags |= (MediaSessionSupportedActionsFlags)action.Key;
                }
            }
            return supportedFlags;
        }

        public override void OnMetadataChanged(MediaMetadata metadata)
        {
            Console.WriteLine("OnMetadataChanged CALLED");

            if (metadata == null) return; //It was proven that it can be null.
            //This method gets called many times,
            //I shoud investigate what is in the Metadata changing so much when I just do something as simple as restart a song.
            //for now, let's compare the information I'm interested in, and if it differs somehow then actually invoke the OnMediaMetadataChanged.

            bool isAnythingDifferent;
            var title = _mediaMetadata?.GetString(MediaMetadata.MetadataKeyTitle);
            var artist = _mediaMetadata?.GetString(MediaMetadata.MetadataKeyArtist);
            var album = _mediaMetadata?.GetString(MediaMetadata.MetadataKeyAlbum);
            var duration = _mediaMetadata?.GetLong(MediaMetadata.MetadataKeyDuration);
            var albumart = _mediaMetadata?.GetBitmap(MediaMetadata.MetadataKeyAlbumArt);


            var incomingTitle = metadata.GetString(MediaMetadata.MetadataKeyTitle);
            var incomingArtist = metadata.GetString(MediaMetadata.MetadataKeyArtist);
            var incomingAlbum = metadata.GetString(MediaMetadata.MetadataKeyAlbum);
            var incomingDuration = metadata.GetLong(MediaMetadata.MetadataKeyDuration);
            var incomingAlbumArt = metadata.GetBitmap(MediaMetadata.MetadataKeyAlbumArt);

            var titleDifferent = title != incomingTitle;
            var artistDifferent = artist != incomingArtist;
            var albumDifferent = album != incomingAlbum;
            var durationDifferent = duration != incomingDuration;
            var albumartDifferent = albumart != null && !albumart.SameAs(incomingAlbumArt);


            isAnythingDifferent = albumartDifferent || titleDifferent || artistDifferent || albumDifferent || durationDifferent;

            if (isAnythingDifferent || resendingMediaMetadata)
            {
                _mediaMetadata = metadata;
                totalProgress = incomingDuration;
                _activityIntent = _mediaController.SessionActivity;

                OnMediaMetadataChanged(new MediaMetadataChangedEventArgs
                {
                    ActivityIntent = _activityIntent,
                    MediaTitle = GetStringValue(MediaMetadata.MetadataKeyTitle),
                    MediaArtist = GetStringValue(MediaMetadata.MetadataKeyArtist),
                    MediaAlbum = GetStringValue(MediaMetadata.MetadataKeyAlbum),
                    MediaDuration = GetLongValue(MediaMetadata.MetadataKeyDuration),
                    MediaArtwork = GetBitmap(MediaMetadata.MetadataKeyAlbumArt),
                    AppName = _appname,
                    PackageName = _mediaController.PackageName
                });
                resendingMediaMetadata = false;
            }

            base.OnMetadataChanged(_mediaMetadata);
        }

        public void OnMediaPlaybackChanged(MediaPlaybackStateChangedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(m =>
            {
                _mediaPlaybackChanged?.Invoke(this, e);
            });
        }

        public void OnMediaMetadataChanged(EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(m =>
            {
                _mediaMetadataChanged?.Invoke(this, (MediaMetadataChangedEventArgs)e);
            });
        }
        public void OnControlsAvailabilityChanged(ControlsAvailabilityChangedEventArgs e)
        {
            _controlsAvailabilityChanged?.Invoke(null, e);
        }

        public bool Finish()
        {
            try
            {
                Console.WriteLine("FINISH CALLED");
                _mediaController.UnregisterCallback(instance);
                _controls.MediaEvent -= MediaEvent;
                CatcherHelper.RequestedOpenNotificationResultGenerated -= CatcherHelper_RequestedOpenNotificationResultGenerated;
                CatcherHelper.NotificationPosted -= CatcherHelper_NotificationPosted;

                //This should let us get rid of the media notification that is holding a this MediaSession, but it's not reliable, given that in some contexts, 
                //pausing the media doesn't cause the Notification to be removable.
                //anyway, this publisher gets unloaded, and for any subscribers the Media Session is discarded (but for android is still going on)
                //This can give us the opportunity to restart the session listening from here, if we haven't removed the notification lol.

                NotificationSlave.GetInstance().CancelNotification(openNotification?.Key);
                progressTimer.Elapsed -= OnProgressTimerElapsed;
                PublisherFinished?.Invoke(null, true);
                
            }
            catch (Exception ex)
            {
                Log.Warn("LiveDisplay", $"Failed Finishing WATCH OUT FOR BAD BEHAVIOR!! {ex.Message}");
                PublisherFinished?.Invoke(null, false);
                return false;
            };
            instance = null;
            return true;
        }
        public override void OnSessionDestroyed()
        {
            Console.WriteLine("SessionDestroyed CALLED");
            //Self destroy instance in this case.
            Finish();
            base.OnSessionDestroyed();
        }

        void TrackProgress(long currentPos, float playbackSpeed)
        {
            currentProgress = currentPos;

            if(playbackSpeed!= 1 && playbackSpeed!=0)
            {
                progressTimer.Interval = OneSecondInMillis / playbackSpeed;
            }

            switch (_playbackState.State)
            {
                case PlaybackStateCode.Playing:
                    progressTimer.Start();
                    break;
                default:
                    progressTimer.Stop();
                    break;
            }
        }
        public void OnProgressTimerElapsed(object sender, EventArgs e)
        {
            currentProgress += OneSecondInMillis;
            OnMediaProgressChanged(new MediaProgressChangedEventArgs
            {
                CurrentProgress = currentProgress,
                TotalProgress = _mediaMetadata.GetLong(MediaMetadata.MetadataKeyDuration)
            });
            if (repeatOptionSet != IMediaEventsPublisher.DontRepeat)
            {
                if (totalProgress - currentProgress <= MillisToRepeat)
                {
                    var mediaControls = MediaControlsBase.Instance;
                    mediaControls?.Pause();
                    mediaControls?.SeekTo(0);
                    mediaControls?.Play();

                    if (repeatOptionSet == IMediaEventsPublisher.RepeatOnce)
                    {
                        repeatOptionSet = IMediaEventsPublisher.DontRepeat;
                    }
                    OnMediaRepeatOptionChanged(repeatOptionSet);
                    Console.WriteLine("REPEATING!!!");
                }
            }
        }

        public void OnMediaProgressChanged(MediaProgressChangedEventArgs e)
        {
            MediaProgressChanged?.Invoke(null, e);
        }
        public void CycleRepeatOption()
        {
            repeatOptionSet++;
            if (repeatOptionSet > IMediaEventsPublisher.RepeatForever)
            {
                repeatOptionSet = IMediaEventsPublisher.DontRepeat;
            }
        }
        public void OnMediaRepeatOptionChanged(int newOption)
        {
            if (resendingRepeatOptionSet) resendingRepeatOptionSet = false;

            MediaRepeatOptionChanged?.Invoke(null, newOption);
        }
        static void ResendSessionInformation(bool resendPlaybackState, bool resendMediaMetadata, bool resendRepeatOptionSet, bool resendingAvailableControls)
        {
            if (instance != null)
            {
                instance.resendingPlaybackState = resendPlaybackState;
                instance.resendingMediaMetadata = resendMediaMetadata;
                instance.resendingRepeatOptionSet = resendRepeatOptionSet;
                instance.resendingAvailableControls = resendingAvailableControls;

                if (instance.resendingMediaMetadata)
                    instance.OnMetadataChanged(instance._mediaMetadata);
                if(instance.resendingPlaybackState)
                    instance.OnPlaybackStateChanged(instance._playbackState);
                if(instance.resendingRepeatOptionSet)
                    instance.OnMediaRepeatOptionChanged(instance.repeatOptionSet);
                if (instance.resendingAvailableControls)
                    instance.OnPlaybackStateChanged(instance._playbackState);


                Console.WriteLine("RESENDING MEDIA INFO");
            }
        }

        public string GetStringValue<TKey>(TKey metadataKey)
        {
            return _mediaMetadata.GetString(metadataKey as string);
        }
        public long GetLongValue<TKey>(TKey metadataKey)
        {
            return _mediaMetadata.GetLong(metadataKey as string);
        }
        public Bitmap GetBitmap<TKey>(TKey metadataKey)
        {
            return _mediaMetadata.GetBitmap(metadataKey as string);
        }

        public AvailableControls SetAvailableControls(MediaSessionSupportedActionsFlags supportedActionsFlags)
        {
            AvailableControls availableControls= new AvailableControls();
            bool wantsToHideSkipToPrev= FindIfMediaSessionWantsToHideDefaultButtons(ExtrasKeySlotReservationSeekToPrev);
            bool wantsToHideSkipToNext= FindIfMediaSessionWantsToHideDefaultButtons(ExtrasKeySlotReservationSeekToNext);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) //TODO: And if the users want to see Android 13+ media controls in previous versions of android,
                                                                    //turns out sometimes they're available if android is not 13+
            {
                //Slot 1.
                if (_playbackState.State == PlaybackStateCode.Stopped
                    || _playbackState.State == PlaybackStateCode.Playing
                     || _playbackState.State == PlaybackStateCode.Paused)
                {
                    availableControls |= AvailableControls.PlayPause;
                }
                if (_playbackState.State == PlaybackStateCode.Buffering)
                {
                    availableControls |= AvailableControls.Buffering;
                }

                //Slot 2: Previous Button
                if (supportedActionsFlags.HasFlag(MediaSessionSupportedActionsFlags.SkipToPrevious) && !wantsToHideSkipToPrev )
                {
                    availableControls |= AvailableControls.SkipToPrevious;
                }
                else
                {
                    Console.WriteLine("SKIP TO PREVIOUS: FILL WITH CUSTOM ACTION");
                    //Fill with one of the custom actions.
                    availableControls |= AvailableControls.SkipToPreviousCustom;
                }

                //Slot 3: Skip To Next button
                if (supportedActionsFlags.HasFlag(MediaSessionSupportedActionsFlags.SkipToNext) && !wantsToHideSkipToNext)
                {
                    //Show the button
                    Console.WriteLine("SHOW SKIP TO NEXT");
                    availableControls |= AvailableControls.SkipToNext;
                }
                else
                {
                    Console.WriteLine("SKIP TO NEXT: FILL WITH CUSTOM ACTION");
                    //Fill with one of the custom actions.
                    availableControls |= AvailableControls.SkipToNextCustom;
                }

                //Slot 4 & 5:
                //Part of the additional controls interface of LiveDisplay.
                //Show the rest of the Custom Actions, if any.
                if(!availableControls.HasFlag(AvailableControls.SkipToPreviousCustom))
                    availableControls |= AvailableControls.CustomActionOne;
                if (!availableControls.HasFlag(AvailableControls.SkipToNextCustom))
                    availableControls |= AvailableControls.CustomActionTwo;


                //LiveDisplay slot 6 :

                if (supportedActionsFlags.HasFlag(MediaSessionSupportedActionsFlags.Stop))
                    availableControls |= AvailableControls.Stop;

                //LiveDisplay slot 7:
               availableControls|= AvailableControls.Repeat;
            }
            else
            {
                //Take them from the Media Style OpenNotification
                //I am assuming that if a media notification gets posted, then it should have the basic controls,
                //Skip to Previous, Play/Pause, and Skip to Next, so we are going to leave our basic controls behave like usual.
                //Instead we're gonna focus on the 
                //Slots 4 and 5, which are the two remaining actions that can be anything.
                //This is Pre-Android 13 behavior

                availableControls|= AvailableControls.PlayPause | 
                    AvailableControls.SkipToNext |
                    AvailableControls.SkipToPrevious|
                    AvailableControls.CustomActionOne | 
                    AvailableControls.CustomActionTwo |
                    AvailableControls.Stop|
                    AvailableControls.Repeat;

                currentAvailableControls = availableControls;
            }
            return availableControls;
        }

    }
}
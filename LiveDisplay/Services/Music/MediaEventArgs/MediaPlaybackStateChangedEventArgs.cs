using Android.Media;
using Android.Media.Session;
using Android.OS;
using System;

namespace LiveDisplay.Services.Media.MediaEventArgs
{
    public class MediaPlaybackStateChangedEventArgs : EventArgs
    {
        private PlaybackStateCode state;

        /// <summary>
        /// Argument indicating the current playback state of the media, playing, stopped, etc-
        /// </summary>
        public PlaybackStateCode PlaybackState 
        {
            get
            {
                return Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch ? GetPlaybackStateCodeFromRemoteControlPlayState(PlaybackStateKitkat) :
                    state;

            }
            set { state = value; } 
        }

        public RemoteControlPlayState PlaybackStateKitkat { private get; set; }

        public long CurrentTime { get; set; }

        public int RepeatOptionSet { get; set; }

        public MediaSessionSupportedActionsFlags SupportedActions { get; set; } = MediaSessionSupportedActionsFlags.None;

        private PlaybackStateCode GetPlaybackStateCodeFromRemoteControlPlayState(RemoteControlPlayState playbackState)
        {
            return playbackState switch
            {
                RemoteControlPlayState.Stopped => PlaybackStateCode.Stopped,
                RemoteControlPlayState.Paused => PlaybackStateCode.Paused,
                RemoteControlPlayState.Playing => PlaybackStateCode.Playing,
                RemoteControlPlayState.FastForwarding => PlaybackStateCode.FastForwarding,
                RemoteControlPlayState.Rewinding => PlaybackStateCode.Rewinding,
                RemoteControlPlayState.Buffering => PlaybackStateCode.Buffering,
                RemoteControlPlayState.Error => PlaybackStateCode.Error,
                RemoteControlPlayState.SkippingBackwards => PlaybackStateCode.SkippingToPrevious,
                RemoteControlPlayState.SkippingForwards => PlaybackStateCode.SkippingToNext,
                

                _ => PlaybackStateCode.None,
            };
        }
    }
}
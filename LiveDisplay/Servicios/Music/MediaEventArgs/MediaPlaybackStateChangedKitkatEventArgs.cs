using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Servicios.Music.MediaEventArgs
{
    class MediaPlaybackStateChangedKitkatEventArgs: EventArgs
    {
       public RemoteControlPlayState PlaybackState { get; set; }
    }
}
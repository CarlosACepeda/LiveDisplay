using Android.Graphics.Drawables;
using System;

namespace LiveDisplay.Services.Battery.BatteryEventArgs
{
    public class BatteryChangedEventArgs : EventArgs
    {
        public int BatteryLevel { get; set; }
        public Drawable BatteryIcon { get; set; }
    }
}
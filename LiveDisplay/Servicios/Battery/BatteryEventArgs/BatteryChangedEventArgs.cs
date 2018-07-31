using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;

namespace LiveDisplay.Servicios.Battery.BatteryEventArgs
{
    public class BatteryChangedEventArgs: EventArgs
    {
        public int BatteryLevel { get; set; }
        public BatteryLevelFlags BatteryLevelFlags { get; set; }
    }
}
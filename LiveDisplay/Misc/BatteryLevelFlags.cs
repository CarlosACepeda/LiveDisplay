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

namespace LiveDisplay.Misc
{
    [Flags]
    public enum BatteryLevelFlags
    {
        OverZero= 0,
        OverTwenty= 1,
        OverThirty= 2,
        OverFifty=3,
        OverSixty= 4,
        OverEighty= 5,
        OverNinety= 6
    }
}
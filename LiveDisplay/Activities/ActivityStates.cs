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

namespace LiveDisplay.Activities
{
    public enum ActivityStates
    {
        Paused= 0,
        Resumed= 1,
        Destroyed= 2,
    }
}
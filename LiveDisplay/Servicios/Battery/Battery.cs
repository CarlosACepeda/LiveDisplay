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

namespace LiveDisplay.Servicios.Battery
{
    /// <summary>
    /// This class will handle the Battery status provided by the BroadcastReceiver and Notify ClockFragment about the changes in battery
    /// </summary>
    class Battery
    {
        private static Battery instance;
        private Battery()
        {
            
        }
        public static Battery BatteryInstance()
        {
            if (instance == null)
            {
                instance = new Battery();
            }
            return instance;
        }


    }
}
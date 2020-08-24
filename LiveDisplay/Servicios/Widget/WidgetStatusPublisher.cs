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

namespace LiveDisplay.Servicios.Widget
{
    //Widgets (fragments) should invoke the event! always, the reason is that lockscreen has to know the state of all the widgets.
    //in order to replace them and stuff.
    public class WidgetStatusPublisher
    {
        private static List<WidgetStatusEventArgs> AllWidgetsStatuses= new List<WidgetStatusEventArgs>();
        public static event EventHandler<WidgetStatusEventArgs> OnWidgetStatusChanged;

        public static void NotifyWidgetStatus(WidgetStatusEventArgs e)
        {            
            var match = AllWidgetsStatuses.Where(x => x.WidgetName == e.WidgetName).FirstOrDefault();
            if (match != null)
            {
                AllWidgetsStatuses.Remove(match);
                AllWidgetsStatuses.Add(e); //Update.
            }
            else 
            {
                AllWidgetsStatuses.Add(e);
            }

            OnWidgetStatusChanged?.Invoke(null, e);
        }
        public static WidgetStatusEventArgs.WidgetStatus? GetWidgetStatus(string nameOfWidget)
        {
            return AllWidgetsStatuses?.Where(x => x.WidgetName == nameOfWidget).FirstOrDefault()?.Status;
        }
    }
    public class WidgetStatusEventArgs : EventArgs
    {
        public string WidgetName { get; set; }
        public WidgetStatus Status { get; set; }
        public enum WidgetStatus
        {
            Unknown = 0,
            Present = 1,
            NonPresent = 2,
            Active= 4 //Should be constantly shown
        }
    }
}

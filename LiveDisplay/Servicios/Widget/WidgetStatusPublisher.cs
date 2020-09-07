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
    //in order to show/hide them.
    public class WidgetStatusPublisher
    {
        public static string CurrentActiveWidget = string.Empty;
        private static List<WidgetStatusEventArgs> AllWidgetsStatuses= new List<WidgetStatusEventArgs>();
        public static event EventHandler<WidgetStatusEventArgs> OnWidgetStatusChanged;

        public static void RequestShow(WidgetStatusEventArgs e)
        {
            if (e.Active)
                CurrentActiveWidget = e.WidgetName;

            OnWidgetStatusChanged?.Invoke(null, e);
        }

    }
    public class WidgetStatusEventArgs : EventArgs
    {
        public string WidgetName { get; set; }
        public bool Show { get; set; }
        public bool Active { get; set; } //It means that if a widget says it is active then 
        //any other widget replacing this one should disappear asap and call RequestShow(CurrentActiveWidget), so the active widget regains control.
    }
}

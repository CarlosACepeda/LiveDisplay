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
    //Esta clase simplemente sirve para guardar el estado actual de la Activity.
    class ActivityLifecycleHelper
    {
        private static bool isActivityVisible;

        //Regresa el estado actual de la Activity.
        public bool IsActivityVisible()
        {
            return isActivityVisible;
        }
        //Actividad resumida, o iniciada.
        public void IsActivityResumed()
        {
            isActivityVisible = true;
        }
        //Actividad Pausada.
        public void IsActivityPaused()
        {
            isActivityVisible = false;
        }
    }
}
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

namespace LiveDisplay.Servicios.FloatingNotification
{
    //Will spawn a View to show the clicked notification in the list, while the music is playing.
    class FloatingNotification : Service
    {
        private View floatingNotificationView;
        private TextView floatingNotificationTitle, floatingNotificationText;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public override void OnCreate()
        {
            
            base.OnCreate();
            IWindowManager windowManager = (IWindowManager)GetSystemService(WindowService);
        }
    }
}
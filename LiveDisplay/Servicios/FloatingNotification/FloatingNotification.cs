using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Servicios.FloatingNotification
{
    //Will spawn a View to show the clicked notification in the list, while the music is playing.
    [Service(Enabled =true)]
    class FloatingNotification : Service
    {
        private IWindowManager windowManager;
        private View floatingNotificationView;
        private TextView floatingNotificationTitle, floatingNotificationText;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }
        public override void OnCreate()
        {
            
            base.OnCreate();
                windowManager = (IWindowManager)GetSystemService(WindowService).JavaCast<IWindowManager>();

            var lol = LayoutInflater.From(this);

            floatingNotificationView = lol.Inflate(Resource.Layout.FloatingNotification, null);

            int lel = 200;
            var floatingNotificationWidth= TypedValue.ApplyDimension(ComplexUnitType.Dip, lel, Resources.DisplayMetrics);

            WindowManagerLayoutParams layoutParams = new WindowManagerLayoutParams();
            layoutParams.Width = (int)floatingNotificationWidth;
            layoutParams.Height = ViewGroup.LayoutParams.WrapContent;
            layoutParams.Type = WindowManagerTypes.Phone;
            layoutParams.Flags = WindowManagerFlags.NotFocusable;
            layoutParams.Format = Android.Graphics.Format.Translucent;
            layoutParams.Gravity = GravityFlags.CenterHorizontal | GravityFlags.CenterVertical;

            windowManager.AddView(floatingNotificationView, layoutParams);
            try
            {
                floatingNotificationView.Click += FloatingNotificationView_Click;
            }
            catch(Exception e)
            {
                Log.Error("LiveDisplay", e.Message);
            }
        }

        private void FloatingNotificationView_Click(object sender, EventArgs e)
        {
            Log.Info("LiveDisplay", "FloatingNotification Clicked");
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            floatingNotificationView.Click -= FloatingNotificationView_Click;
            if (floatingNotificationView != null)
            {
                windowManager.RemoveView(floatingNotificationView);
            }
        }
    }
}
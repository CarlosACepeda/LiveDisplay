using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDisplay.Servicios.Notificaciones.NotificationStyle
{
    class BigTextStyleNotification : NotificationStyle
    {
        public BigTextStyleNotification(OpenNotification openNotification, ref LinearLayout notificationView, AndroidX.Fragment.App.Fragment notificationFragment)
      : base(openNotification, ref notificationView, notificationFragment)
        {

        }
        protected override void SetTextMaxLines()
        {
            Text.SetMaxLines(9);
        }
        protected override void Collapse_Click(object sender, EventArgs e)
        {
            Text.Visibility = Text.Visibility != ViewStates.Visible ? ViewStates.Visible : ViewStates.Invisible;
        }

    }
}
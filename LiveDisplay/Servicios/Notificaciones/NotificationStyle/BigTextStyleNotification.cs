using Android.Views;
using Android.Widget;
using System;

namespace LiveDisplay.Servicios.Notificaciones.NotificationStyle
{
    internal class BigTextStyleNotification : NotificationStyle
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
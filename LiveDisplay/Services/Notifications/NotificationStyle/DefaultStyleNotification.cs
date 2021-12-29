using Android.Widget;
using LiveDisplay.Models;

namespace LiveDisplay.Services.Notifications.NotificationStyle
{
    public class DefaultStyleNotification : NotificationStyle
    {
        public DefaultStyleNotification(OpenNotification openNotification, ref LinearLayout notificationView, AndroidX.Fragment.App.Fragment notificationFragment)
            : base(openNotification, ref notificationView, notificationFragment)
        {
        }
    }
}
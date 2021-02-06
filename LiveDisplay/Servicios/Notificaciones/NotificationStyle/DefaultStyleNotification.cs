using Android.Widget;

namespace LiveDisplay.Servicios.Notificaciones.NotificationStyle
{
    public class DefaultStyleNotification : NotificationStyle
    {
        public DefaultStyleNotification(OpenNotification openNotification, ref LinearLayout notificationView, AndroidX.Fragment.App.Fragment notificationFragment)
            : base(openNotification, ref notificationView, notificationFragment)
        {
        }
    }
}
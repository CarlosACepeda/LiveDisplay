using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Net.Wifi.Aware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Servicios.Notificaciones.NotificationStyle
{   
    public interface IStyle
    {
        public enum NotificationType
        {
            OnLockscreen,
            Floating
        }
        public void ApplyStyle(NotificationType notificationType, ref OpenNotification openNotification);        
    }
}
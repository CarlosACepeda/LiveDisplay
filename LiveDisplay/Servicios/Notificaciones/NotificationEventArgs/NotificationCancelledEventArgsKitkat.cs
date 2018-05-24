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

namespace LiveDisplay.Servicios.Notificaciones.NotificationEventArgs
{
    class NotificationCancelledEventArgsKitkat : EventArgs
    {
        public string NotificationPackage { get; set; }
        public string NotificationTag { get; set; }
        public int NotificationId { get; set; }
    }
}
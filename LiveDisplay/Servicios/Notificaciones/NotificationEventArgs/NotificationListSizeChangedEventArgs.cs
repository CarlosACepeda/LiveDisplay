﻿using System;
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
    class NotificationListSizeChangedEventArgs: EventArgs
    {
        /// <summary>
        /// Boolean to check if the Notification list has notifications
        /// </summary>
        public bool ThereAreNotifications { get; set; }
    }
}
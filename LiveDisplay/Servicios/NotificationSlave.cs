using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Service.Notification;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Servicios
{
    class NotificationSlave
    {
        //Postear Notificaciones sobre mi app.
        NotificationManager notificationManager = (NotificationManager)Application.Context.GetSystemService("notification");
        //Instancia a Catcher para cancelar notificaciones desde la lockScreen
        public void CancelNotification(string notiPack, string notiTag, int notiId)
        {
            //Deprecated
            Catcher.catcherInstance.CancelNotification(notiPack, notiTag, notiId);
        }
        public void CancelAll()
        {
            Catcher.catcherInstance.CancelAllNotifications();
        }
    }
}
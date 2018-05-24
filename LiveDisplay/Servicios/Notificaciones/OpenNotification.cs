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
using Java.IO;

namespace LiveDisplay.Servicios.Notificaciones
{
    class OpenNotification: IDisposable
    {
        int position;


        public OpenNotification(int position)
        { 
            this.position = position;
        }
        //TODO
        public string GetTitle()
        {
            return CatcherHelper.statusBarNotifications[position].Notification.Extras.Get("android.title").ToString();
        }
        public string GetText()
        {
            try
            {
                return CatcherHelper.statusBarNotifications[position].Notification.Extras.Get("android.text").ToString();
            }
            catch
            {
                return "";
            }
        }

        public void ClickNotification(int position)
        {
            var pendingIntent = CatcherHelper.statusBarNotifications[position].Notification.ContentIntent;
            try
            {
                pendingIntent.Send();
            }
            catch
            {
                System.Console.WriteLine("Click Notification failed, fail in pending intent");
            }
            pendingIntent.Dispose();
        }
        public void RetrieveActionButtons(int position)
        {
            List<Button> buttons = new List<Button>();

        }

        public void Dispose()
        {
            
        }
        
    }
}
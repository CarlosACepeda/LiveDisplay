using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Servicios.Notificaciones.NotificationStyle
{
    public class MessagingStyle : NormalNotification, IStyle
    {
        //Note: the notification view 
        public MessagingStyle(View notificationView)
        {
            Title = notificationView.FindViewById<TextView>(Resource.Id.tvTitulo);
            Text = notificationView.FindViewById<TextView>(Resource.Id.tvTexto);
            Subtext = notificationView.FindViewById<TextView>(Resource.Id.tvnotifSubtext);
            ApplicationName = notificationView.FindViewById<TextView>(Resource.Id.tvAppName);
            When = notificationView.FindViewById<TextView>(Resource.Id.tvWhen);
            CloseNotificationButton = notificationView.FindViewById<ImageButton>(Resource.Id.tvWhen);
            InlineResponseContainer = notificationView.FindViewById<LinearLayout>(Resource.Id.inlineNotificationContainer);
            InlineResponse = notificationView.FindViewById<EditText>(Resource.Id.tvInlineText);
            SendInlineResponse = notificationView.FindViewById<ImageButton>(Resource.Id.sendInlineResponseButton);
            NotificationProgress = notificationView.FindViewById<ProgressBar>(Resource.Id.notificationprogress);
        }
        public void ApplyStyle(IStyle.NotificationType notificationType, ref OpenNotification openNotification)
        {
            
        }
    }
}
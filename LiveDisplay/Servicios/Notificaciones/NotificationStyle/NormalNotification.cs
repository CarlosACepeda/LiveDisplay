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

namespace LiveDisplay.Servicios.Notificaciones.NotificationStyle
{
    //Represents a definition of what a notification view has in general.
    public abstract class NormalNotification
    {
        public TextView Title { get; set; }
        public TextView Text { get; set; }
        public TextView Subtext { get; set; }
        public TextView ApplicationName { get; set; }
        public TextView When { get; set; }
        public ImageButton CloseNotificationButton { get; set; }
        public LinearLayout InlineResponseContainer { get; set; }
        public EditText InlineResponse { get; set; }
        public ImageButton SendInlineResponse { get; set; }
        public ProgressBar NotificationProgress { get; set; }
        public ImageView Icon { get; set; }        
    }
}
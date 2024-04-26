using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LiveDisplay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDisplay.Services.Notifications.NotificationStyle
{
    class DecoratedCustomViewStyle: NotificationStyle
    {

        public DecoratedCustomViewStyle(OpenNotification openNotification, ref LinearLayout notificationView, AndroidX.Fragment.App.Fragment notificationFragment)
: base(openNotification, ref notificationView, notificationFragment)
        {

        }
        public override void ApplyStyle()
        {
            base.ApplyStyle();
            var view = OpenNotification.CustomView.Apply(NotificationFragment.Context, null);
            view.Tag = CUSTOM_VIEW_TAG;
            ActualNotification.AddView(view, 2); 
            //Index two because at index 0 we have icons, at index 1 we have the close button and app name, etc, index 2 is for the actual notification content.
            //See Notification layout file.
        }
    }
}
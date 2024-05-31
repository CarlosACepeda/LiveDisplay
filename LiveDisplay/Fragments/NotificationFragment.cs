using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Services;
using LiveDisplay.Services.Notifications;
using LiveDisplay.Services.Notifications.NotificationEventArgs;
using LiveDisplay.Services.Widget;
using System;

using Fragment = AndroidX.Fragment.App.Fragment;

namespace LiveDisplay.Fragments
{
    public class NotificationFragment : Fragment
    {
        OpenNotification openNotification; //the current OpenNotification instance active.
        LinearLayout maincontainer;
        TextView app_name, when, subtext, title, text;
        AppCompatImageButton action1, action2, action3, action4, action5;

        #region Lifecycle events

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.Notification, container, false);

            maincontainer = v.FindViewById<LinearLayout>(Resource.Id.container);
            app_name = v.FindViewById<TextView>(Resource.Id.app_name);
            when = v.FindViewById<TextView>(Resource.Id.when);
            subtext = v.FindViewById<TextView>(Resource.Id.subtext);
            title = v.FindViewById<TextView>(Resource.Id.title);
            text = v.FindViewById<TextView>(Resource.Id.text);



            maincontainer.Drag += Notification_Drag;
            maincontainer.Click += LlNotification_Click;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;
            return v;
        }

        private void Notification_Drag(object sender, View.DragEventArgs e)
        {

        }

        private void CatcherHelper_NotificationPosted(object sender, NotificationPostedEventArgs e)
        {
            if (e.OpenNotification.Style == OpenNotification.MessagingStyle)
            {
                app_name.Text = e.OpenNotification.AppName;
                when.Text = e.OpenNotification.When;
                subtext.Text = e.OpenNotification.SubText;
                title.Text = e.OpenNotification.Title;
                text.Text = e.OpenNotification.Text;
            }
        }
        public override void OnDestroyView()
        {
            //NotificationAdapterViewHolder.ItemLongClicked -= ItemLongClicked;
            CatcherHelper.NotificationRemoved -= CatcherHelper_NotificationRemoved;
            CatcherHelper.NotificationPosted -= CatcherHelper_NotificationPosted;

            base.OnDestroyView();
        }

        public override void OnDestroy()
        {
            openNotification?.Dispose();
            

            base.OnDestroy();
        }

        #endregion Lifecycle events

        #region Events Implementation:

        private void CatcherHelper_NotificationRemoved(object sender, NotificationRemovedEventArgs e)
        {
            //Activity?.RunOnUiThread(() =>
            //{
            //    maincontainer.Visibility = ViewStates.Gone;
            //    //Remove tag, notification removed
            //    openNotification = null;
            //    maincontainer?.SetTag(Resource.String.defaulttag, null);
            //});
        }

        private void LlNotification_Click(object sender, EventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                //try
                //{
                //    Activity?.RunOnUiThread(() => openNotification.ClickNotification());
                //    if (openNotification.IsAutoCancellable())
                //    {
                //        WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "NotificationFragment" });
                //        maincontainer.Visibility = ViewStates.Invisible;
                //    }
                //}
                //catch
                //{
                //    Log.Wtf("OnNotificationClicked", "Metodo falla porque no existe una notificacion con esta acción");
                //}
            });
        }

        private void ItemLongClicked(object sender, NotificationItemClickedEventArgs e)
        {
            maincontainer.Visibility = ViewStates.Visible;
            openNotification = e.OpenNotification;
            //openNotification.Cancel();
            WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "NotificationFragment" });
            maincontainer.Visibility = ViewStates.Invisible;
        }

        private void ItemClicked(object sender, NotificationItemClickedEventArgs e)
        {
            openNotification =e.OpenNotification;

            //if the current notification widget does not have a tag, let's set it.

            //if (maincontainer.GetTag(Resource.String.defaulttag) == null)
            //{
            //    maincontainer.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
            //}

            //Only do this process if the notification that I want to show is different than the one that
            //the Notification Widget has.
            //If it's the same then simply show it.
            //if ((string)maincontainer.GetTag(Resource.String.defaulttag) != openNotification.GetCustomId())
            //{
            //    styleApplier?.ApplyStyle(openNotification);
            //    maincontainer.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
            //    if (maincontainer.Visibility != ViewStates.Visible)
            //    {
            //        WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = true, WidgetName = "NotificationFragment" });
            //        maincontainer.Visibility = ViewStates.Visible;
            //    }
            //}
            //else
            //{
            //    styleApplier?.ApplyStyle(openNotification);
            //    WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = true, WidgetName = "NotificationFragment" });
            //    maincontainer.Visibility = ViewStates.Visible;
            //}
            StartTimeout(false);
        }

        #endregion Events Implementation:

        //THis works like a charm :)
        private void StartTimeout(bool stop)
        {
            //This action is: 'Hide the notification, and set the timeoutStarted as finished(false)
            //because this action will be invoked only when the timeout has finished.
            
            //If the timeout has started, then cancel the action, and start again.

            //if (stop)
            //{
            //    maincontainer?.RemoveCallbacks(HideNotification); //Stop counting.
            //    return;
            //}
            //else
            //{
            //    if (timeoutStarted == true)
            //    {
            //        maincontainer?.RemoveCallbacks(HideNotification);
            //        maincontainer?.PostDelayed(HideNotification,7000);
            //    }
            //    //If not, simply wait 5 seconds then hide the notification, in that span of time, the timeout is
            //    //marked as Started(true)
            //    else
            //    {
            //        timeoutStarted = true;
            //        maincontainer?.PostDelayed(HideNotification, 7000);
            //    }
            //}
        }
    }
}
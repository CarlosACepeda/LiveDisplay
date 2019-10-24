using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using LiveDisplay.Servicios.Notificaciones.NotificationStyle;
using System;

namespace LiveDisplay.Fragments
{
    public class NotificationFragment : Fragment
    {
        private OpenNotification openNotification; //the current OpenNotification instance active.
        private LinearLayout notification;
        private bool timeoutStarted = false;
        private NotificationStyleApplier styleApplier;

        #region Lifecycle events

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.NotificationFrag, container, false);
            notification = v.FindViewById<LinearLayout>(Resource.Id.llNotification); 
            styleApplier = new NotificationStyleApplier(ref notification);
            
            notification.Drag += Notification_Drag;
            notification.Click += LlNotification_Click;
            NotificationAdapterViewHolder.ItemClicked += ItemClicked;
            NotificationAdapterViewHolder.ItemLongClicked += ItemLongClicked;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationUpdated += CatcherHelper_NotificationUpdated;
            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;
            return v;
        }


        private void Notification_Drag(object sender, View.DragEventArgs e)
        {
            StartTimeout(); //To keep the notification visible while the user touches the notification fragment
        }

        private void CatcherHelper_NotificationPosted(object sender, NotificationPostedEventArgs e)
        {
            if (e.ShouldCauseWakeUp == true)
            {
                Awake.WakeUpScreen();
            }
        }

        public override void OnDestroy()
        {
            NotificationAdapterViewHolder.ItemClicked -= ItemClicked;
            NotificationAdapterViewHolder.ItemLongClicked -= ItemLongClicked;
            CatcherHelper.NotificationUpdated -= CatcherHelper_NotificationUpdated;
            CatcherHelper.NotificationRemoved -= CatcherHelper_NotificationRemoved;
            CatcherHelper.NotificationPosted -= CatcherHelper_NotificationPosted;
            openNotification?.Dispose();
            base.OnDestroy();
        }

        #endregion Lifecycle events

        #region Events Implementation:

        private void CatcherHelper_NotificationRemoved(object sender, EventArgs e)
        {
            notification.Visibility = ViewStates.Gone;
        }

        private void CatcherHelper_NotificationUpdated(object sender, NotificationItemClickedEventArgs e)
        {
            ItemClicked(this, e);
        }

        private void LlNotification_Click(object sender, EventArgs e)
        {
            notification.Visibility = ViewStates.Visible;
            try
            {
                Activity.RunOnUiThread(() => openNotification.ClickNotification());
                if (openNotification.IsAutoCancellable())
                {
                    notification.Visibility = ViewStates.Invisible;
                }

            }
            catch
            {
                Log.Wtf("OnNotificationClicked", "Metodo falla porque no existe una notificacion con esta acción");
            }
        }

        private void ItemLongClicked(object sender, NotificationItemClickedEventArgs e)
        {
            notification.Visibility = ViewStates.Visible;
            openNotification = new OpenNotification(e.StatusBarNotification);
            openNotification.Cancel();
             notification.Visibility = ViewStates.Invisible;
            
        }

        private void ItemClicked(object sender, NotificationItemClickedEventArgs e)
        {
            openNotification = new OpenNotification(e.StatusBarNotification);
            Toast.MakeText(Activity, openNotification.GetGroupInfo(), ToastLength.Short).Show();
            //Watch out for possible memory leaks here.
            styleApplier?.ApplyStyle(openNotification);

            if (notification.Visibility != ViewStates.Visible)
            {
                notification.Visibility = ViewStates.Visible;
                StartTimeout();
            }

        }

        #endregion Events Implementation:

        //THis works like a charm :)
        private void StartTimeout()
        {
            //This action is: 'Hide the notification, and set the timeoutStarted as finished(false)
            //because this action will be invoked only when the timeout has finished.
            void hideNotification() { if (notification != null) notification.Visibility = ViewStates.Gone; timeoutStarted = false; }
            //If the timeout has started, then cancel the action, and start again.
            if (timeoutStarted == true)
            {
                notification?.RemoveCallbacks(hideNotification);
                notification?.PostDelayed(hideNotification, 5000);
            }
            //If not, simply wait 5 seconds then hide the notification, in that span of time, the timeout is
            //marked as Started(true)
            else
            {
                timeoutStarted = true;
                notification?.PostDelayed(hideNotification, 5000);
            }
        }
    }
}
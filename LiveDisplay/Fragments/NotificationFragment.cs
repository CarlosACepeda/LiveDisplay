using Android.App;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Misc;
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
        private ConfigurationManager configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context));       

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
            styleApplier = new NotificationStyleApplier(ref notification, this);
            
            notification.Drag += Notification_Drag;
            notification.Click += LlNotification_Click;
            NotificationAdapterViewHolder.ItemClicked += ItemClicked;
            NotificationAdapterViewHolder.ItemLongClicked += ItemLongClicked;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;
            NotificationStyleApplier.SendInlineResponseAvailabityChanged += NotificationStyleApplier_SendInlineResponseAvailabityChanged;
            return v;
        }

        private void NotificationStyleApplier_SendInlineResponseAvailabityChanged(object sender, bool e)
        {
            if (e == true)
            {
                StartTimeout(true); //Tell the Timeout counter to stop because the SendInlineResponse is currently being showed.
            }
        }

        private void Notification_Drag(object sender, View.DragEventArgs e)
        {
            StartTimeout(false); //To keep the notification visible while the user touches the notification fragment
        }

        private void CatcherHelper_NotificationPosted(object sender, NotificationPostedEventArgs e)
        {
            openNotification = new OpenNotification(e.StatusBarNotification);

            //if the current notification widget does not have a tag, let's set it.

            if (notification.GetTag(Resource.String.defaulttag) == null)
            {
                notification.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
            }

            if (configurationManager.RetrieveAValue(ConfigurationParameters.TestEnabled))
            {
                Toast.MakeText(Activity, "Progress Indeterminate?: " + openNotification.IsProgressIndeterminate().ToString() + "\n"
                    + "Current Progress: " + openNotification.GetProgress().ToString() + "\n"
                    + "Max Progress: " + openNotification.GetProgressMax().ToString() + "\n"
                    + openNotification.GetGroupInfo()
                    , ToastLength.Short).Show();
            }

            if (e.UpdatesPreviousNotification)
            {
                Activity.RunOnUiThread(() =>
                {
                    //if updates a previous notification, let's see if first of all the notification
                    //to be updated is the same that's currently being displayed in the Notification Widget.
                    if ((string)notification.GetTag(Resource.String.defaulttag) == openNotification.GetCustomId())
                    {

                        //Watch out for possible memory leaks here.
                        styleApplier?.ApplyStyle(openNotification);

                        //let's attach a tag to the fragment in order to know which notification is this fragment showing.
                        notification.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());

                        if (notification.Visibility != ViewStates.Visible)
                        {
                            notification.Visibility = ViewStates.Visible;
                            StartTimeout(false);
                        }

                    }
                    else
                    {
                        //they are not the same so, the notification widget won't get updated(because that'll cause the 
                        //notification the user is viewing to be replaced)
                    }
                });

            }
            else
            {
                Activity.RunOnUiThread(() =>
                {
                    styleApplier?.ApplyStyle(openNotification);
                    notification.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
                    if (notification.Visibility != ViewStates.Visible)
                    {
                        notification.Visibility = ViewStates.Visible;
                        StartTimeout(false);
                    }
                });
            }
        }

        public override void OnDestroy()
        {
            NotificationAdapterViewHolder.ItemClicked -= ItemClicked;
            NotificationAdapterViewHolder.ItemLongClicked -= ItemLongClicked;
            CatcherHelper.NotificationRemoved -= CatcherHelper_NotificationRemoved;
            CatcherHelper.NotificationPosted -= CatcherHelper_NotificationPosted;
            openNotification?.Dispose();
            base.OnDestroy();
        }

        #endregion Lifecycle events

        #region Events Implementation:

        private void CatcherHelper_NotificationRemoved(object sender, EventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                notification.Visibility = ViewStates.Gone;
                //Remove tag, notification removed
                openNotification = null;
                notification?.SetTag(Resource.String.defaulttag, null);
            });
        }
        private void LlNotification_Click(object sender, EventArgs e)
        {
            Activity?.RunOnUiThread(() =>
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
            });
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

            //if the current notification widget does not have a tag, let's set it.

            if (notification.GetTag(Resource.String.defaulttag) == null)
            {
                notification.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
            }

            if (configurationManager.RetrieveAValue(ConfigurationParameters.TestEnabled))
            {
                Toast.MakeText(Activity, "Progress Indeterminate?: " + openNotification.IsProgressIndeterminate().ToString() + "\n"
                    + "Current Progress: " + openNotification.GetProgress().ToString() + "\n"
                    + "Max Progress: " + openNotification.GetProgressMax().ToString() + "\n"
                    + openNotification.GetGroupInfo()
                    , ToastLength.Short).Show();
            }

            //Only do this process if the notification that I want to show is different than the one that 
            //the Notification Widget has.
            //If it's the same then simply show it.
            if ((string)notification.GetTag(Resource.String.defaulttag) != openNotification.GetCustomId())
            {
                styleApplier?.ApplyStyle(openNotification);
                notification.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
                if (notification.Visibility != ViewStates.Visible)
                {
                    notification.Visibility = ViewStates.Visible;                    
                }
            }
            else if(notification.Visibility!= ViewStates.Visible)
            {
                styleApplier?.ApplyStyle(openNotification);
                notification.Visibility = ViewStates.Visible;                
            }
            StartTimeout(false);

        }


        #endregion Events Implementation:

        //THis works like a charm :)
        private void StartTimeout(bool stop)
        {            
            //This action is: 'Hide the notification, and set the timeoutStarted as finished(false)
            //because this action will be invoked only when the timeout has finished.
            void hideNotification() { if (notification != null) notification.Visibility = ViewStates.Gone; timeoutStarted = false; }
            //If the timeout has started, then cancel the action, and start again.

            if (stop)
            {
                notification?.RemoveCallbacks(hideNotification); //Stop counting.
                return;
            }
            else
            {

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
}
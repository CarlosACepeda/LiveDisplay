using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Awake;
using LiveDisplay.Servicios.Music;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using LiveDisplay.Servicios.Notificaciones.NotificationStyle;
using LiveDisplay.Servicios.Widget;
using System;

using Fragment = AndroidX.Fragment.App.Fragment;

namespace LiveDisplay.Fragments
{
    public class NotificationFragment : Fragment
    {
        private OpenNotification openNotification; //the current OpenNotification instance active.
        private LinearLayout maincontainer;
        private bool timeoutStarted = false;
        private NotificationStyleApplier styleApplier;
        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
        #region Lifecycle events

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            NotificationAdapterViewHolder.ItemClicked += ItemClicked;
            // Create your fragment here
            Activity.RunOnUiThread(() => Toast.MakeText(Context, "NotifFragment: OnCreate", ToastLength.Long).Show());
            WidgetStatusPublisher.OnWidgetStatusChanged += WidgetStatusPublisher_OnWidgetStatusChanged;

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.NotificationFrag, container, false);
            maincontainer = v.FindViewById<LinearLayout>(Resource.Id.container);
            styleApplier = new NotificationStyleApplier(ref maincontainer, this, NotificationViewType.OnLockscreen);
            maincontainer.Drag += Notification_Drag;
            maincontainer.Click += LlNotification_Click;
            NotificationAdapterViewHolder.ItemLongClicked += ItemLongClicked;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;
            NotificationStyleApplier.SendInlineResponseAvailabityChanged += NotificationStyleApplier_SendInlineResponseAvailabityChanged;

            //if (openNotification == null) //We don't have a notification to show here, so...
            //{
            //    //...Now ask Catcher to send us the last notification posted to fill the views..
            //    NotificationSlave.NotificationSlaveInstance().RetrieveLastNotification();
            //}
            Activity.RunOnUiThread(() => Toast.MakeText(Context, "NotifFragment: OnCreateView", ToastLength.Long).Show());
            return v;
        }
        public override void OnPause()
        {
            Activity.RunOnUiThread(() => Toast.MakeText(Context, "NotifFragment: OnPause", ToastLength.Long).Show());
            base.OnPause();
        }
        public override void OnResume()
        {
            Activity.RunOnUiThread(() => Toast.MakeText(Context, "NotifFragment: OnResume", ToastLength.Long).Show());

            base.OnResume();
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
            openNotification = e.OpenNotification;
            if (e.ShouldCauseWakeUp)
                AwakeHelper.TurnOnScreen();

            if (e.OpenNotification.RepresentsMediaPlaying())
            {
                MusicController.StartPlayback(e.OpenNotification.GetMediaSessionToken());
                
                maincontainer.Visibility = ViewStates.Invisible;
                WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "NotificationFragment" });

                //Also start the Widget to control the playback.
                WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = true, WidgetName = "MusicFragment", Active= true });
                return; 
            }


            //if the current notification widget does not have a tag, let's set it.

            if (maincontainer.GetTag(Resource.String.defaulttag) == null)
            {
                maincontainer.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
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
                    if ((string)maincontainer.GetTag(Resource.String.defaulttag) == openNotification.GetCustomId())
                    {
                        //Watch out for possible memory leaks here.
                        styleApplier?.ApplyStyle(openNotification);

                        //let's attach a tag to the fragment in order to know which notification is this fragment showing.
                        maincontainer.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());

                        if (maincontainer.Visibility != ViewStates.Visible)
                        {
                            WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = true, WidgetName = "NotificationFragment" });
                            maincontainer.Visibility = ViewStates.Visible;
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
                    maincontainer.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
                    if (maincontainer.Visibility != ViewStates.Visible)
                    {
                        WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = true, WidgetName = "NotificationFragment" });
                        maincontainer.Visibility = ViewStates.Visible;
                        StartTimeout(false);
                    }
                });
            }
        }
        public override void OnDestroyView()
        {
            //notification.Drag -= Notification_Drag;
            //notification.Click -= LlNotification_Click;
            //NotificationAdapterViewHolder.ItemLongClicked -= ItemLongClicked;
            //CatcherHelper.NotificationRemoved -= CatcherHelper_NotificationRemoved;
            //CatcherHelper.NotificationPosted -= CatcherHelper_NotificationPosted;
            //NotificationStyleApplier.SendInlineResponseAvailabityChanged -= NotificationStyleApplier_SendInlineResponseAvailabityChanged;

            styleApplier = null;
            Activity.RunOnUiThread(() => Toast.MakeText(Context, "NotifFragment: OnDestroyView", ToastLength.Long).Show());

            base.OnDestroyView();
        }

        public override void OnDestroy()
        {
            openNotification?.Dispose();
            Activity.RunOnUiThread(() => Toast.MakeText(Context, "NotifFragment: OnDestroy", ToastLength.Long).Show());
            NotificationAdapterViewHolder.ItemClicked -= ItemClicked;
            WidgetStatusPublisher.OnWidgetStatusChanged -= WidgetStatusPublisher_OnWidgetStatusChanged;

            base.OnDestroy();
        }

        private void WidgetStatusPublisher_OnWidgetStatusChanged(object sender, WidgetStatusEventArgs e)
        {
            if (e.WidgetName == "MusicFragment")
            {
                if (e.Show == true)
                {
                    if (maincontainer != null)
                        maincontainer.Visibility = ViewStates.Invisible;
                }
            }
        }

        #endregion Lifecycle events

        #region Events Implementation:

        private void CatcherHelper_NotificationRemoved(object sender, EventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "NotificationFragment" });

                maincontainer.Visibility = ViewStates.Gone;
                //Remove tag, notification removed
                openNotification = null;
                maincontainer?.SetTag(Resource.String.defaulttag, null);
            });
        }

        private void LlNotification_Click(object sender, EventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                try
                {
                    Activity.RunOnUiThread(() => openNotification.ClickNotification());
                    if (openNotification.IsAutoCancellable())
                    {
                        WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "NotificationFragment" });
                        maincontainer.Visibility = ViewStates.Invisible;
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
            maincontainer.Visibility = ViewStates.Visible;
            openNotification = new OpenNotification(e.StatusBarNotification);
            openNotification.Cancel();
            WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "NotificationFragment" });
            maincontainer.Visibility = ViewStates.Invisible;
        }

        private void ItemClicked(object sender, NotificationItemClickedEventArgs e)
        {
            openNotification = new OpenNotification(e.StatusBarNotification);

            //if the current notification widget does not have a tag, let's set it.

            if (maincontainer.GetTag(Resource.String.defaulttag) == null)
            {
                maincontainer.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
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
            if ((string)maincontainer.GetTag(Resource.String.defaulttag) != openNotification.GetCustomId())
            {
                styleApplier?.ApplyStyle(openNotification);
                maincontainer.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
                if (maincontainer.Visibility != ViewStates.Visible)
                {
                    WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = true, WidgetName = "NotificationFragment" });
                    maincontainer.Visibility = ViewStates.Visible;
                }
            }
            else
            {
                styleApplier?.ApplyStyle(openNotification);
                WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = true, WidgetName = "NotificationFragment" });
                maincontainer.Visibility = ViewStates.Visible;
            }
            StartTimeout(false);
        }

        #endregion Events Implementation:

        //THis works like a charm :)
        private void StartTimeout(bool stop)
        {
            //This action is: 'Hide the notification, and set the timeoutStarted as finished(false)
            //because this action will be invoked only when the timeout has finished.
            
            //If the timeout has started, then cancel the action, and start again.

            if (stop)
            {
                maincontainer?.RemoveCallbacks(HideNotification); //Stop counting.
                return;
            }
            else
            {
                if (timeoutStarted == true)
                {
                    maincontainer?.RemoveCallbacks(HideNotification);
                    maincontainer?.PostDelayed(HideNotification,7000);
                }
                //If not, simply wait 5 seconds then hide the notification, in that span of time, the timeout is
                //marked as Started(true)
                else
                {
                    timeoutStarted = true;
                    maincontainer?.PostDelayed(HideNotification, 7000);
                }
            }
        }
        void HideNotification()
        {
            if (maincontainer != null)
            {
                maincontainer.Visibility = ViewStates.Gone;
                timeoutStarted = false;
                WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "NotificationFragment" });
            }
        }
    }
}
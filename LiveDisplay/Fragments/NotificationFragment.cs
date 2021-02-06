using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Awake;
using LiveDisplay.Servicios.Keyguard;
using LiveDisplay.Servicios.Music;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using LiveDisplay.Servicios.Notificaciones.NotificationStyle;
using LiveDisplay.Servicios.Widget;
using System;
using System.Threading;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace LiveDisplay.Fragments
{
    public class NotificationFragment : Fragment
    {
        private const string BigPictureStyle = "android.app.Notification$BigPictureStyle";
        private const string InboxStyle = "android.app.Notification$InboxStyle";
        private const string MediaStyle = "android.app.Notification$MediaStyle";
        public const string MessagingStyle = "android.app.Notification$MessagingStyle"; //Only available on API Level 24 and up.
        private const string BigTextStyle = "android.app.Notification$BigTextStyle";
        private const string DecoratedCustomViewStyle = "android.app.Notification$DecoratedCustomViewStyle";

        private OpenNotification _openNotification; //the current OpenNotification instance active.
        private LinearLayout maincontainer;
        private bool timeoutStarted = false;
        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        #region Lifecycle events

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            NotificationAdapterViewHolder.ItemClicked += ItemClicked;
            // Create your fragment here
            WidgetStatusPublisher.OnWidgetStatusChanged += WidgetStatusPublisher_OnWidgetStatusChanged;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.NotificationFrag, container, false);
            maincontainer = v.FindViewById<LinearLayout>(Resource.Id.container);
            maincontainer.Drag += Notification_Drag;
            maincontainer.Click += LlNotification_Click;
            NotificationAdapterViewHolder.ItemLongClicked += ItemLongClicked;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;
            NotificationStyle.SendInlineResponseAvailabityChanged += NotificationStyleApplier_SendInlineResponseAvailabityChanged;

            //if (openNotification == null) //We don't have a notification to show here, so...
            //{
            //    //...Now ask Catcher to send us the last notification posted to fill the views..
            //    NotificationSlave.NotificationSlaveInstance().RetrieveLastNotification();
            //}
            return v;
        }

        public override void OnPause()
        {
            base.OnPause();
        }

        public override void OnResume()
        {
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
            ShowNotification(e.OpenNotification);

            if (e.ShouldCauseWakeUp && configurationManager.RetrieveAValue(ConfigurationParameters.TurnOnUserMovement))
                AwakeHelper.TurnOnScreen();

            if (configurationManager.RetrieveAValue(ConfigurationParameters.MusicWidgetMethod, "0") == "1") //1:"Use a notification to spawn the Music Widget"
            {
                if (e.OpenNotification.RepresentsMediaPlaying())
                {
                    MusicController.StartPlayback(e.OpenNotification.GetMediaSessionToken(), _openNotification.GetCustomId());

                    maincontainer.Visibility = ViewStates.Invisible;
                    WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "NotificationFragment" });

                    //Also start the Widget to control the playback.
                    WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = true, WidgetName = "MusicFragment", Active = true });
                    return;
                }
            }
        }

        public override void OnDestroyView()
        {
            //maincontainer.Drag -= Notification_Drag;
            //maincontainer.Click -= LlNotification_Click;
            NotificationAdapterViewHolder.ItemLongClicked -= ItemLongClicked;
            CatcherHelper.NotificationRemoved -= CatcherHelper_NotificationRemoved;
            CatcherHelper.NotificationPosted -= CatcherHelper_NotificationPosted;
            NotificationStyle.SendInlineResponseAvailabityChanged -= NotificationStyleApplier_SendInlineResponseAvailabityChanged;

            base.OnDestroyView();
        }

        public override void OnDestroy()
        {
            _openNotification?.Dispose();

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
            if (e.WidgetName == "NotificationFragment" && e.WidgetAskingForShowing == "MusicFragment" && e.AdditionalInfo != null)
            {
                ShowNotification(CatcherHelper.GetOpenNotification((string)e.AdditionalInfo));
            }
        }

        #endregion Lifecycle events

        #region Events Implementation:

        private void CatcherHelper_NotificationRemoved(object sender, NotificationRemovedEventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                if (configurationManager.RetrieveAValue(ConfigurationParameters.MusicWidgetMethod, "0") == "1")
                {
                    if (e.OpenNotification.RepresentsMediaPlaying())
                    {
                        ThreadPool.QueueUserWorkItem(m =>
                        {
                            MusicController.StopPlayback(e.OpenNotification.GetMediaSessionToken());
                        } //Returns true if the Playback was stopped succesfully (Sometimes it wont work)
                        );

                        //In any case, order MusicWidget to stop.
                        WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "MusicFragment", Active = false });
                    }
                }

                WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "NotificationFragment" });

                maincontainer.Visibility = ViewStates.Gone;
                //Remove tag, notification removed
                _openNotification = null;
                maincontainer?.SetTag(Resource.String.defaulttag, null);
            });
        }

        private void LlNotification_Click(object sender, EventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                try
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        if (KeyguardHelper.IsDeviceCurrentlyLocked())
                        {
                            KeyguardHelper.RequestDismissKeyguard(Activity);
                        }
                    Activity?.RunOnUiThread(() => _openNotification.ClickNotification());
                    if (_openNotification.IsAutoCancellable())
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
            e.StatusBarNotification.Cancel();
            WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "NotificationFragment" });
            maincontainer.Visibility = ViewStates.Invisible;
        }

        private void ItemClicked(object sender, NotificationItemClickedEventArgs e)
        {
            ShowNotification(e.StatusBarNotification);
        }

        public void ShowNotification(OpenNotification openNotification)
        {
            _openNotification = openNotification;

            Activity.RunOnUiThread(() =>
            {
                if (configurationManager.RetrieveAValue(ConfigurationParameters.TestEnabled))
                {
                    Toast.MakeText(Application.Context, "Progress Indeterminate?: " + _openNotification.IsProgressIndeterminate().ToString() + "\n"
                        + "Current Progress: " + _openNotification.GetProgress().ToString() + "\n"
                        + "Max Progress: " + _openNotification.GetProgressMax().ToString() + "\n"
                        + _openNotification.GetGroupInfo()
                        , ToastLength.Short).Show();
                }
                //Determine if the notification to show updates the current view,(in case there's a notification currently owning this Widget)
                bool updating = _openNotification.GetId() == openNotification.GetId();

                switch (_openNotification.Style())
                {
                    case BigPictureStyle:
                        new BigPictureStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    case MessagingStyle:
                        new MessagingStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    case InboxStyle:
                        new InboxStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    case BigTextStyle:
                        new BigTextStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    case MediaStyle:
                        new MediaStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    default:
                        new DefaultStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;
                }

                StartTimeout(false);

                //Now we check if the current showing widget is this, if not, ask for us to be the current showing widget.
                if (WidgetStatusPublisher.CurrentActiveWidget != "NotificationFragment")
                {
                    WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = true, WidgetName = "NotificationFragment" });
                    maincontainer.Visibility = ViewStates.Visible; //we make ourselves visible when we are the current showing widget.
                }
                else if (maincontainer.Visibility != ViewStates.Visible) maincontainer.Visibility = ViewStates.Visible;
            });
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
                    maincontainer?.PostDelayed(HideNotification, 7000);
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

        private void HideNotification()
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
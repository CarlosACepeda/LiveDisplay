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
using System.Collections.Generic;
using System.Linq;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace LiveDisplay.Fragments
{
    public class NotificationFragment : Fragment
    {        
        const int FIVE_SECONDS = 5;

        private OpenNotification _openNotification; //the current OpenNotification instance active.
        private LinearLayout maincontainer;
        private bool timeoutStarted = false;
        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        private List<OpenNotification> Notifications = new List<OpenNotification>();

        #region Lifecycle events

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            NotificationAdapter.ItemClick += ItemClicked;
            // Create your fragment here
            WidgetStatusPublisher.GetInstance().OnWidgetStatusChanged += WidgetStatusPublisher_OnWidgetStatusChanged;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.test_notif_view, container, false);
            maincontainer = v.FindViewById<LinearLayout>(Resource.Id.notification_container);
            maincontainer.Drag += Notification_Drag;
            maincontainer.Click += LlNotification_Click;
            NotificationAdapter.ItemLongClick += ItemLongClicked;
            NotificationAdapter.NotificationPosted += CatcherHelper_NotificationPosted;
            NotificationAdapter.NotificationRemoved += CatcherHelper_NotificationRemoved;
            NotificationStyle.SendInlineResponseAvailabityChanged += NotificationStyleApplier_SendInlineResponseAvailabityChanged;

            //if (openNotification == null) //We don't have a notification to show here, so...
            //{
            //    //...Now ask Catcher to send us the last notification posted to fill the views..
            //    NotificationSlave.NotificationSlaveInstance().RetrieveLastNotification();
            //}
            return v;
        }

        private void NotificationStyleApplier_SendInlineResponseAvailabityChanged(object sender, bool e)
        {
            if (e)
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
            Notifications = e.OpenNotifications;
            OpenNotification openNotification = GetNotificationById(e.NotificationPostedId);
            //if the incoming notification updates a previous notification, then verify if the current notification SHOWING is the same as the one
            //we are trying to update, because if this check is not done, the updated notification will show even if the user is seeing another notification.
            //the other case is simply when the notification is a new one.
            if(e.UpdatesPreviousNotification && openNotification.GetCustomId()== _openNotification?.GetCustomId()
                || e.UpdatesPreviousNotification== false)
            {
                ShowNotification(openNotification);
            }

            if (e.ShouldCauseWakeUp && configurationManager.RetrieveAValue(ConfigurationParameters.TurnOnNewNotification))
                AwakeHelper.TurnOnScreen();
        }

        public override void OnDestroyView()
        {
            NotificationAdapter.ItemLongClick -= ItemLongClicked;
            NotificationAdapter.NotificationRemoved -= CatcherHelper_NotificationRemoved;
            NotificationAdapter.NotificationPosted -= CatcherHelper_NotificationPosted;
            NotificationStyle.SendInlineResponseAvailabityChanged -= NotificationStyleApplier_SendInlineResponseAvailabityChanged;

            base.OnDestroyView();
        }

        public override void OnDestroy()
        {
            _openNotification?.Dispose();

            NotificationAdapter.ItemClick -= ItemClicked;
            WidgetStatusPublisher.GetInstance().OnWidgetStatusChanged -= WidgetStatusPublisher_OnWidgetStatusChanged;
            ToggleWidgetVisibility(false);
            base.OnDestroy();
        }

        private void WidgetStatusPublisher_OnWidgetStatusChanged(object sender, WidgetStatusEventArgs e)
        {
            if (e.WidgetName == Constants.NOTIFICATION_FRAGMENT)
            {
                if (e.Show)
                {
                    ToggleWidgetVisibility(true);
                }
                else
                    ToggleWidgetVisibility(false);
            }
            else ToggleWidgetVisibility(false);
        }
        private void ToggleWidgetVisibility(bool visible)
        {
            if (maincontainer != null)
            {
                if (visible)
                    maincontainer.Visibility = ViewStates.Visible;
                else
                    maincontainer.Visibility = ViewStates.Gone;
            }
        }

        #endregion Lifecycle events

        #region Events Implementation:

        private void CatcherHelper_NotificationRemoved(object sender, NotificationRemovedEventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                WidgetStatusPublisher.GetInstance().SetWidgetVisibility(new ShowParameters { Show = false, WidgetName = Constants.NOTIFICATION_FRAGMENT });

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
                    if (_openNotification.IsAutoCancellable)
                    {
                        WidgetStatusPublisher.GetInstance().SetWidgetVisibility(new ShowParameters { Show = false, WidgetName = Constants.NOTIFICATION_FRAGMENT });
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
            WidgetStatusPublisher.GetInstance().SetWidgetVisibility(new ShowParameters { Show = false, WidgetName = Constants.NOTIFICATION_FRAGMENT });
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
                    Toast.MakeText(Application.Context, "Progress Indeterminate?: " + _openNotification.ProgressIndeterminate.ToString() + "\n"
                        + "Current Progress: " + _openNotification.Progress.ToString() + "\n"
                        + "Max Progress: " + _openNotification.MaximumProgress.ToString() + "\n"
                        + _openNotification.GetGroupInfo()
                        , ToastLength.Short).Show();
                }
                switch (_openNotification.Style)
                {
                    case Constants.BIG_PICTURE_STYLE:
                        new BigPictureStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    case Constants.MESSAGING_STYLE:
                        new MessagingStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    case Constants.INBOX_STYLE:
                        new InboxStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    case Constants.BIG_TEXT_STYLE:
                        new BigTextStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    case Constants.MEDIA_STYLE:
                        new MediaStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    default:
                        new DefaultStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;
                }

                StartTimeout(false);

                WidgetStatusPublisher.GetInstance().SetWidgetVisibility(
                    new ShowParameters { 
                        Show = true, Active= true, WidgetName = Constants.NOTIFICATION_FRAGMENT, TimeToShow= FIVE_SECONDS
                    });
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
                WidgetStatusPublisher.GetInstance().SetWidgetVisibility(new ShowParameters { Show = false, WidgetName = Constants.NOTIFICATION_FRAGMENT });
            }
        }

        OpenNotification GetNotificationById(int id)
        {
            return Notifications.Where(o => o.Id == id).FirstOrDefault();
        }
        bool IsNotificationSummary(int id)
        {
            OpenNotification summaryNotification;
            summaryNotification = Notifications.Where(o => o.Id == id).FirstOrDefault();

            if (summaryNotification != null)
                return summaryNotification.IsSummary;

            return false;
        }
    }
}
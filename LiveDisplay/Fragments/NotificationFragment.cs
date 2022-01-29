using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Enums;
using LiveDisplay.Misc;
using LiveDisplay.Models;
using LiveDisplay.Services;
using LiveDisplay.Services.Awake;
using LiveDisplay.Services.Keyguard;
using LiveDisplay.Services.Music;
using LiveDisplay.Services.Notifications;
using LiveDisplay.Services.Notifications.NotificationEventArgs;
using LiveDisplay.Services.Notifications.NotificationStyle;
using LiveDisplay.Services.Widget;
using System;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace LiveDisplay.Fragments
{
    public class NotificationFragment : Fragment
    {
        const int SEVEN_SECONDS = 7;

        private OpenNotification _openNotification; //the current active OpenNotification instance.
        private LinearLayout maincontainer;
        private LinearLayout actual_notification;
        private readonly ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

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
            actual_notification = v.FindViewById<LinearLayout>(Resource.Id.actual_notification);
            maincontainer.Drag += Notification_Drag;
            actual_notification.Click += ActualNotification_Click;
            NotificationAdapter.ItemLongClick += ItemLongClicked;
            NotificationAdapter.NotificationPosted += NotificationAdapter_NotificationPosted;
            NotificationAdapter.NotificationRemoved += NotificationAdapter_NotificationRemoved;
            NotificationStyle.SendInlineResponseAvailabityChanged += NotificationStyleApplier_SendInlineResponseAvailabityChanged;
            return v;
        }

        private void NotificationStyleApplier_SendInlineResponseAvailabityChanged(object sender, bool available)
        {
            if (available)
            {
                WidgetStatusPublisher.GetInstance().SetWidgetVisibility(
                    new ShowParameters
                    {
                        Show = true,
                        WidgetName = WidgetTypes.NOTIFICATION_FRAGMENT,
                        TimeToShow = ShowParameters.ACTIVE_PERMANENTLY
                    });
            }
            else
            {
                WidgetStatusPublisher.GetInstance().SetWidgetVisibility(
                    new ShowParameters
                    {
                        Show = true,
                        WidgetName = WidgetTypes.NOTIFICATION_FRAGMENT,
                        TimeToShow = SEVEN_SECONDS
                    });
            }
        }

        private void Notification_Drag(object sender, View.DragEventArgs e)
        {
            WidgetStatusPublisher.GetInstance().SetWidgetVisibility(
                    new ShowParameters
                    {
                        Show = true,
                        WidgetName = WidgetTypes.NOTIFICATION_FRAGMENT,
                        TimeToShow = SEVEN_SECONDS
                    });
        }

        private void NotificationAdapter_NotificationPosted(object sender, NotificationPostedEventArgs e)
        {
            //if the incoming notification updates a previous notification, then verify if the current notification SHOWING is the same as the one
            //we are trying to update, because if this check is not done, the updated notification will show even if the user is seeing another notification.
            //the other case is simply when the notification is a new one.
            if(e.UpdatesPreviousNotification && e.NotificationPosted.GetCustomId== _openNotification?.GetCustomId && !MusicController.MediaSessionAssociatedWThisNotification(_openNotification?.GetCustomId)
                || !e.UpdatesPreviousNotification)
            {
                ShowNotification(e.NotificationPosted);
            }

            if (!e.UpdatesPreviousNotification && e.ShouldCauseWakeUp && configurationManager.RetrieveAValue(ConfigurationParameters.TurnOnNewNotification))
                AwakeHelper.TurnOnScreen();
        }

        public override void OnDestroyView()
        {
            NotificationAdapter.ItemLongClick -= ItemLongClicked;
            NotificationAdapter.NotificationRemoved -= NotificationAdapter_NotificationRemoved;
            NotificationAdapter.NotificationPosted -= NotificationAdapter_NotificationPosted;
            NotificationStyle.SendInlineResponseAvailabityChanged -= NotificationStyleApplier_SendInlineResponseAvailabityChanged;
            maincontainer.Drag -= Notification_Drag;
            actual_notification.Click -= ActualNotification_Click;

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
            if (e.WidgetName == WidgetTypes.NOTIFICATION_FRAGMENT)
            {
                if (e.Show)
                {
                    if(e.AdditionalInfo!= null)
                    {
                        //TODO: Standarize the type of info presented here.
                        //When this isn't null, it carries a notification id. to search for that notification
                        //and show it here. but it could be something else and cause a crash. 
                        ShowNotification(NotificationHijackerWorker.GetOpenNotification(e.AdditionalInfo.ToString()));
                    }
                    ToggleWidgetVisibility(true);
                }
                else
                {
                    ToggleWidgetVisibility(false);
                }
            }
        }
        private void ToggleWidgetVisibility(bool visible)
        {
            Activity.RunOnUiThread(() => {
                if (maincontainer != null)
                {
                    if (visible)
                    {
                        maincontainer.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        maincontainer.Visibility = ViewStates.Invisible;
                    }
                }

            });
        }

        #endregion Lifecycle events

        #region Events Implementation:

        private void NotificationAdapter_NotificationRemoved(object sender, NotificationRemovedEventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                WidgetStatusPublisher.GetInstance().SetWidgetVisibility(new ShowParameters { Show = false, WidgetName = WidgetTypes.NOTIFICATION_FRAGMENT });

                //Remove tag, notification removed
                _openNotification = null;
                maincontainer?.SetTag(Resource.String.defaulttag, null);
            });
        }

        private void ActualNotification_Click(object sender, EventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                try
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O && KeyguardHelper.IsDeviceCurrentlyLocked())
                        KeyguardHelper.RequestDismissKeyguard(Activity);

                    Activity?.RunOnUiThread(() => NotificationHijackerWorker.ClickNotification(_openNotification));
                    if (_openNotification.IsAutoCancellable)
                    {
                        WidgetStatusPublisher.GetInstance().SetWidgetVisibility(new ShowParameters { Show = false, WidgetName = WidgetTypes.NOTIFICATION_FRAGMENT });
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
            //Notificatione.StatusBarNotification.Cancel();
            //WidgetStatusPublisher.GetInstance().SetWidgetVisibility(new ShowParameters { Show = false, WidgetName = WidgetTypes.NOTIFICATION_FRAGMENT });
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
                    case NotificationStyles.BIG_PICTURE_STYLE:
                        new BigPictureStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    case NotificationStyles.MESSAGING_STYLE:
                        new MessagingStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    case NotificationStyles.INBOX_STYLE:
                        new InboxStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;
                    case NotificationStyles.BIG_TEXT_STYLE:
                        new BigTextStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    case NotificationStyles.MEDIA_STYLE:
                        new MediaStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;

                    default:
                        new DefaultStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;
                }

                WidgetStatusPublisher.GetInstance().SetWidgetVisibility(
                    new ShowParameters { 
                        Show = true, WidgetName = WidgetTypes.NOTIFICATION_FRAGMENT, TimeToShow= SEVEN_SECONDS
                    });
            });
        }

        #endregion Events Implementation:
    }
}
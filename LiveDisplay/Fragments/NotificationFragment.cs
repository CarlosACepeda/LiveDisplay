using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
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
using System.Collections.Generic;
using static AndroidX.RecyclerView.Widget.RecyclerView;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace LiveDisplay.Fragments
{
    public class NotificationFragment : Fragment
    {
        const int SEVEN_SECONDS = 7;

        private OpenNotification _openNotification; //the current active OpenNotification instance.
        private LinearLayout maincontainer;
        private LinearLayout actual_notification;
        private RecyclerView children_notifications;
        private readonly ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

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
            children_notifications = v.FindViewById<RecyclerView>(Resource.Id.children_notifications);

            var layoutManager = new LinearLayoutManager(Application.Context, Vertical, false);
            children_notifications.SetLayoutManager(layoutManager);
            

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
            if(e.IsParent)
            {
                ToggleChildrenVisibility(false);
                LoadChildrenNotifications(e.Children);
            }
            if(e.IsSibling)
            {
                //TODO: Check what's more convenient, to alert the user with the updated/posted parent notification
                //that contains a tiny fraction of the actual posted notification, without expanding the children list, or show the actual notification that was posted/ updated by scrolling the children recycler view to
                ////the notification that was posted/updated, meaning that it'll expand children list that this group has.

                //We pass the group adapter the notification id we want to show.
                ToggleChildrenVisibility(true);
                var childrenNotificationsAdapter = new NotificationGroupAdapter(e.Children, e.NotificationPosted.Id);
                children_notifications.SetAdapter(childrenNotificationsAdapter);
                children_notifications.SmoothScrollToPosition(childrenNotificationsAdapter.notificationToShowPosition);
            }
            if (e.IsStandalone)
            {
                children_notifications.Visibility = ViewStates.Gone;
                //As always, just be sure to not show the Grouped notifications recycler view, is not needed.
            }



            //if the incoming notification updates a previous notification, then verify if the current SHOWING notification is the same as the one
            //we are trying to update, because if this check is not done, the updated notification will show even if the user is watching another notification.
            //the other case is simply when the notification is a new one.
            if (e.UpdatesPreviousNotification && IsUpdatingSameNotificationUserIsViewing(e.NotificationPosted.GetCustomId)
                && !MusicController.MediaSessionAssociatedWThisNotification(e.NotificationPosted.GetCustomId)
                || 
                !e.UpdatesPreviousNotification)
            {
                ShowNotification(e.NotificationPosted, 0, null); // todo: fix or find a better way/
            }

            if (!e.UpdatesPreviousNotification && e.ShouldCauseWakeUp && configurationManager.RetrieveAValue(ConfigurationParameters.TurnOnNewNotification))
                AwakeHelper.TurnOnScreen();
        }

        void ToggleChildrenVisibility(bool visible)
        {
            if(children_notifications != null)
                children_notifications.Visibility = visible? ViewStates.Visible: ViewStates.Gone;
        }
        void LoadChildrenNotifications(List<OpenNotification>children)
        {
            children_notifications.SetAdapter(new NotificationGroupAdapter(children));
        }

        bool IsUpdatingSameNotificationUserIsViewing(string openNotificationCustomId)
        {
            //This is because user hasn't seen any notifications yet (hence the null value), so we'll say, yes, update it, it doesn't matter.
            if (_openNotification == null) return true; 

            return _openNotification.GetCustomId == openNotificationCustomId;
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
                        OpenNotification notification = NotificationHijackerWorker.GetOpenNotification(e.AdditionalInfo.ToString());
                        if(notification!= null)
                        {
                            ShowNotification(notification, 0, null);
                        }
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
            ShowNotification(e.StatusBarNotification, e.ChildCount ,e.Children);
        }

        public void ShowNotification(OpenNotification openNotification, int childrenCount, List<OpenNotification> children)
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
                    case NotificationStyles.DECORATED_CUSTOM_VIEW_STYLE:
                        new DecoratedCustomViewStyle(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;
                    default:
                        new DefaultStyleNotification(_openNotification, ref maincontainer, this).ApplyStyle();
                        break;
                }

                if(childrenCount>0)
                {
                    children_notifications.SetAdapter(new NotificationGroupAdapter(children));
                }

                WidgetStatusPublisher.GetInstance().SetWidgetVisibility(
                    new ShowParameters { 
                        Show = true, WidgetName = WidgetTypes.NOTIFICATION_FRAGMENT, TimeToShow= SEVEN_SECONDS
                    });
            });
        }
    }
}
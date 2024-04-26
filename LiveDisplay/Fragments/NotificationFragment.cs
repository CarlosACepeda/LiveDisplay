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
                if (e.OpenNotification.RepresentsMediaPlaying())
                {
                    MediaEventsPublisherLollipop.InitializeFromToken(e.OpenNotification.GetMediaSessionToken());

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
                //maincontainer.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
            }

            if (!e.UpdatesPreviousNotification && e.ShouldCauseWakeUp && configurationManager.RetrieveAValue(ConfigurationParameters.TurnOnNewNotification))
                AwakeHelper.TurnOnScreen();
        }

            if (e.UpdatesPreviousNotification)
            {
                Activity?.RunOnUiThread(() =>
                {
                    //if updates a previous notification, first of all let's see if the notification
                    //to be updated is the same that's currently being displayed in the Notification Widget.
                    //if ((string)maincontainer.GetTag(Resource.String.defaulttag) == openNotification.GetCustomId())
                    //{
                    //    //Watch out for possible memory leaks here.
                    //    styleApplier?.ApplyStyle(openNotification);

                    //    //let's attach a tag to the fragment in order to know which notification is this fragment showing.
                    //    maincontainer.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());

                    //    if (maincontainer.Visibility != ViewStates.Visible)
                    //    {
                    //        WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = true, WidgetName = "NotificationFragment" });
                    //        maincontainer.Visibility = ViewStates.Visible;
                    //        StartTimeout(false);
                    //    }
                    //}
                    //else
                    //{
                    //    //they are not the same so, the notification widget won't get updated(because that'll cause the
                    //    //notification the user is viewing to be replaced)
                    //}
                });
            }
            else
            {
                Activity?.RunOnUiThread(() =>
                {
                    styleApplier?.ApplyStyle(openNotification);
                    //maincontainer.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
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

        private void CatcherHelper_NotificationRemoved(object sender, NotificationRemovedEventArgs e)
        {
            Activity?.RunOnUiThread(() =>
            {
                if (configurationManager.RetrieveAValue(ConfigurationParameters.MusicWidgetMethod, "0") == "1")
                {
                    if (e.OpenNotification.RepresentsMediaPlaying())
                    {
                        if (MediaEventsPublisherLollipop.GetInstance().Finish(e.OpenNotification.GetMediaSessionToken())) //Returns true if the Playback was stopped succesfully
                        {
                            //In that case, order MusicWidget to stop.
                            WidgetStatusPublisher.RequestShow(new WidgetStatusEventArgs { Show = false, WidgetName = "MusicFragment", Active = false });
                        }
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
            maincontainer.Visibility = ViewStates.Visible;
            openNotification = e.OpenNotification;
            openNotification.Cancel();
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

        public void ShowNotification(OpenNotification openNotification, int childrenCount, List<OpenNotification> children)
        {
            _openNotification = openNotification;

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
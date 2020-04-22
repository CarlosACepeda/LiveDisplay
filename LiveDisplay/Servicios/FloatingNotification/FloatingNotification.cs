using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Activities;
using LiveDisplay.Adapters;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Awake;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using System;

namespace LiveDisplay.Servicios.FloatingNotification
{
    //Maybe the service should allow also showing the list of notifications?
    //it'll be cool to detach the notifications' list from the lockscreen and show them while the user does not use the lockscreen, or even the music widget, a floating one.

    //Will spawn a View to show the clicked notification in the list, while the music is playing.
    [Service(Enabled = true)]
    internal class FloatingNotification : Service, View.IOnTouchListener
    {
        private IWindowManager windowManager;
        private LinearLayout floatingNotificationView;
        private TextView floatingNotificationTitle;
        private TextView floatingNotificationText;
        private TextView floatingNotificationAppName;
        private TextView floatingNotificationWhen;
        private LinearLayout floatingNotificationActionsContainer;
        private OpenNotification openNotification; //Represents the openNotification instance corresponding with this floating notification.
        private ActivityStates currentActivityState;

        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            WindowManagerTypes layoutType = WindowManagerTypes.Phone;

            if (Build.VERSION.SdkInt > BuildVersionCodes.NMr1) //Nougat 7.1
            {
                layoutType = WindowManagerTypes.ApplicationOverlay; //Android Oreo does not allow to add windows of WindowManagerTypes.Phone
            }

            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();

            var lol = LayoutInflater.From(this);

            floatingNotificationView = (LinearLayout)lol.Inflate(Resource.Layout.FloatingNotification, null);

            int width = 200;
            var floatingNotificationWidth = TypedValue.ApplyDimension(ComplexUnitType.Dip, width, Resources.DisplayMetrics);

            WindowManagerLayoutParams layoutParams = new WindowManagerLayoutParams
            {
                Width = (int)floatingNotificationWidth,
                Height = ViewGroup.LayoutParams.WrapContent,
                Type = layoutType,
                Flags = WindowManagerFlags.NotFocusable | WindowManagerFlags.WatchOutsideTouch | WindowManagerFlags.ShowWhenLocked,
                Format = Format.Translucent,
                Gravity = GravityFlags.CenterHorizontal | GravityFlags.CenterVertical
            };
            floatingNotificationView.Visibility = ViewStates.Gone;

            windowManager.AddView(floatingNotificationView, layoutParams);

            floatingNotificationAppName = floatingNotificationView.FindViewById<TextView>(Resource.Id.floatingappname);
            floatingNotificationWhen = floatingNotificationView.FindViewById<TextView>(Resource.Id.floatingwhen);
            floatingNotificationTitle = floatingNotificationView.FindViewById<TextView>(Resource.Id.floatingtitle);
            floatingNotificationText = floatingNotificationView.FindViewById<TextView>(Resource.Id.floatingtext);
            floatingNotificationActionsContainer = floatingNotificationView.FindViewById<LinearLayout>(Resource.Id.floatingNotificationActions);

            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            NotificationAdapterViewHolder.ItemClicked += NotificationAdapterViewHolder_ItemClicked;
            NotificationAdapterViewHolder.ItemLongClicked += NotificationAdapterViewHolder_ItemLongClicked;
            LockScreenActivity.OnActivityStateChanged += LockScreenActivity_OnActivityStateChanged;
            floatingNotificationView.SetOnTouchListener(this);
        }

        private void LockScreenActivity_OnActivityStateChanged(object sender, Activities.ActivitiesEventArgs.LockScreenLifecycleEventArgs e)
        {
            switch (e.State)
            {
                case ActivityStates.Paused:
                    if (floatingNotificationView.Visibility == ViewStates.Visible)
                        floatingNotificationView.Visibility = ViewStates.Invisible;

                    break;

                case ActivityStates.Resumed:
                    //?
                    break;

                case ActivityStates.Destroyed:
                    if (floatingNotificationView.Visibility == ViewStates.Visible)
                        floatingNotificationView.Visibility = ViewStates.Invisible;

                    break;

                default:
                    break;
            }
            currentActivityState = e.State;
        }

        private void CatcherHelper_NotificationPosted(object sender, NotificationPostedEventArgs e)
        {
            openNotification = e.OpenNotification;
            if (e.ShouldCauseWakeUp)
                AwakeHelper.TurnOnScreen();


            //if the current floating notification widget does not have a tag, let's set it.

            if (floatingNotificationView.GetTag(Resource.String.defaulttag) == null)
            {
                floatingNotificationView.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
            }

            if (configurationManager.RetrieveAValue(ConfigurationParameters.TestEnabled))
            {
                Toast.MakeText(floatingNotificationView.Context, "Progress Indeterminate?: " + openNotification.IsProgressIndeterminate().ToString() + "\n"
                    + "Current Progress: " + openNotification.GetProgress().ToString() + "\n"
                    + "Max Progress: " + openNotification.GetProgressMax().ToString() + "\n"
                    + openNotification.GetGroupInfo()
                    , ToastLength.Short).Show();
            }

            if (e.UpdatesPreviousNotification)
            {
                if ((string)floatingNotificationView.GetTag(Resource.String.defaulttag) == openNotification.GetCustomId())
                {
                    floatingNotificationAppName.Text = openNotification.AppName();
                    floatingNotificationWhen.Text = openNotification.When();
                    floatingNotificationTitle.Text = openNotification.Title();
                    floatingNotificationText.Text = openNotification.Text();
                    floatingNotificationActionsContainer.RemoveAllViews();

                    if (openNotification.HasActions() == true)
                    {
                        var actions = openNotification.RetrieveActions();
                        foreach (var a in actions)
                        {
                            OpenAction openAction = new OpenAction(a);
                            float weight = (float)1 / actions.Count;

                            Button anActionButton = new Button(Application.Context)
                            {
                                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent, weight),
                                Text = openAction.Title(),
                            };
                            anActionButton.SetTypeface(Typeface.Create("sans-serif-condensed", TypefaceStyle.Normal), TypefaceStyle.Normal);
                            anActionButton.SetMaxLines(1);
                            anActionButton.SetTextColor(Color.Black);
                            anActionButton.Click += (o, eventargs) =>
                            {
                                openAction.ClickAction();
                            };
                            anActionButton.Gravity = GravityFlags.CenterVertical;
                            TypedValue outValue = new TypedValue();
                            Application.Context.Theme.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackgroundBorderless, outValue, true);
                            anActionButton.SetBackgroundResource(outValue.ResourceId);
                            //anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                            floatingNotificationActionsContainer.AddView(anActionButton);
                        };
                    }
                }
            }
            else
            {
                //Is a new notification, so set a new tag.
                floatingNotificationView.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());

                floatingNotificationAppName.Text = openNotification.AppName();
                floatingNotificationWhen.Text = openNotification.When();
                floatingNotificationTitle.Text = openNotification.Title();
                floatingNotificationText.Text = openNotification.Text();
                floatingNotificationActionsContainer.RemoveAllViews();

                if (openNotification.HasActions() == true)
                {
                    var actions = openNotification.RetrieveActions();
                    foreach (var a in actions)
                    {
                        OpenAction openAction = new OpenAction(a);
                        float weight = (float)1 / actions.Count;

                        Button anActionButton = new Button(Application.Context)
                        {
                            LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent, weight),
                            Text = openAction.Title(),
                        };
                        anActionButton.SetTypeface(Typeface.Create("sans-serif-condensed", TypefaceStyle.Normal), TypefaceStyle.Normal);
                        anActionButton.SetMaxLines(1);
                        anActionButton.SetTextColor(Color.Black);
                        anActionButton.Click += (o, eventargs) =>
                        {
                            openAction.ClickAction();
                        };
                        anActionButton.Gravity = GravityFlags.CenterVertical;
                        TypedValue outValue = new TypedValue();
                        Application.Context.Theme.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackgroundBorderless, outValue, true);
                        anActionButton.SetBackgroundResource(outValue.ResourceId);
                        //anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                        floatingNotificationActionsContainer.AddView(anActionButton);
                    };
                }

                if (floatingNotificationView.Visibility != ViewStates.Visible)
                {
                    if (currentActivityState == ActivityStates.Resumed) //most of the times it won't work, when the Screen turns on then it gets here too quickly
                        //before the lockscreen is in a Resumed state, causing the floating notification not being showed when the screen turns on, TODO.
                        floatingNotificationView.Visibility = ViewStates.Visible;
                }
            }
        }

        private void CatcherHelper_NotificationRemoved(object sender, EventArgs e)
        {
            floatingNotificationView.Visibility = ViewStates.Gone;
            //Remove tag, notification removed
            openNotification = null;
            floatingNotificationView?.SetTag(Resource.String.defaulttag, null);
        }

        private void NotificationAdapterViewHolder_ItemLongClicked(object sender, NotificationItemClickedEventArgs e)
        {
            openNotification = new OpenNotification(e.StatusBarNotification);
            openNotification.Cancel();
            floatingNotificationView.Visibility = ViewStates.Gone;
        }

        private void NotificationAdapterViewHolder_ItemClicked(object sender, NotificationItemClickedEventArgs e)
        {
            openNotification = new OpenNotification(e.StatusBarNotification);

            if (configurationManager.RetrieveAValue(ConfigurationParameters.TestEnabled))
            {
                Toast.MakeText(floatingNotificationView.Context, "Progress Indeterminate?: " + openNotification.IsProgressIndeterminate().ToString() + "\n"
                    + "Current Progress: " + openNotification.GetProgress().ToString() + "\n"
                    + "Max Progress: " + openNotification.GetProgressMax().ToString() + "\n"
                    + openNotification.GetGroupInfo()
                    , ToastLength.Short).Show();
            }
            //Only do this process if the notification that I want to show is different than the one that
            //the Floating Notification Widget has.
            //If it's the same then simply show it.
            if ((string)floatingNotificationView.GetTag(Resource.String.defaulttag) != openNotification.GetCustomId())
            {
                floatingNotificationAppName.Text = openNotification.AppName();
                floatingNotificationWhen.Text = openNotification.When();
                floatingNotificationTitle.Text = openNotification.Title();
                floatingNotificationText.Text = openNotification.Text();
                floatingNotificationActionsContainer.RemoveAllViews();

                if (openNotification.HasActions() == true)
                {
                    var actions = openNotification.RetrieveActions();
                    foreach (var a in actions)
                    {
                        OpenAction openAction = new OpenAction(a);
                        float weight = (float)1 / actions.Count;

                        Button anActionButton = new Button(Application.Context)
                        {
                            LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent, weight),
                            Text = openAction.Title(),
                        };
                        anActionButton.SetTypeface(Typeface.Create("sans-serif-condensed", TypefaceStyle.Normal), TypefaceStyle.Normal);
                        anActionButton.SetMaxLines(1);
                        anActionButton.SetTextColor(Color.Black);
                        anActionButton.Click += (o, eventargs) =>
                        {
                            openAction.ClickAction();
                        };
                        anActionButton.Gravity = GravityFlags.CenterVertical;
                        TypedValue outValue = new TypedValue();
                        Application.Context.Theme.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackgroundBorderless, outValue, true);
                        anActionButton.SetBackgroundResource(outValue.ResourceId);
                        //anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                        floatingNotificationActionsContainer.AddView(anActionButton);
                    };
                }
            }
            else
            {
                floatingNotificationAppName.Text = openNotification.AppName();
                floatingNotificationWhen.Text = openNotification.When();
                floatingNotificationTitle.Text = openNotification.Title();
                floatingNotificationText.Text = openNotification.Text();
                floatingNotificationActionsContainer.RemoveAllViews();

                if (openNotification.HasActions() == true)
                {
                    var actions = openNotification.RetrieveActions();
                    foreach (var a in actions)
                    {
                        OpenAction openAction = new OpenAction(a);
                        float weight = (float)1 / actions.Count;

                        Button anActionButton = new Button(Application.Context)
                        {
                            LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent, weight),
                            Text = openAction.Title(),
                        };
                        anActionButton.SetTypeface(Typeface.Create("sans-serif-condensed", TypefaceStyle.Normal), TypefaceStyle.Normal);
                        anActionButton.SetMaxLines(1);
                        anActionButton.SetTextColor(Color.Black);
                        anActionButton.Click += (o, eventargs) =>
                        {
                            openAction.ClickAction();
                        };
                        anActionButton.Gravity = GravityFlags.CenterVertical;
                        TypedValue outValue = new TypedValue();
                        Application.Context.Theme.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackgroundBorderless, outValue, true);
                        anActionButton.SetBackgroundResource(outValue.ResourceId);
                        //anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                        floatingNotificationActionsContainer.AddView(anActionButton);
                    };
                }
            }
            if (floatingNotificationView.Visibility != ViewStates.Visible)
            {
                floatingNotificationView.Visibility = ViewStates.Visible;
            }
            else if (floatingNotificationView.Visibility != ViewStates.Visible)
            {
                floatingNotificationView.Visibility = ViewStates.Invisible;
            }
        }

        private void FloatingNotificationView_Click(object sender, EventArgs e)
        {
            openNotification.ClickNotification();
            floatingNotificationView.Visibility = ViewStates.Gone;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            LockScreenActivity.OnActivityStateChanged -= LockScreenActivity_OnActivityStateChanged;
            floatingNotificationView.Click -= FloatingNotificationView_Click;
            NotificationAdapterViewHolder.ItemClicked -= NotificationAdapterViewHolder_ItemClicked;
            NotificationAdapterViewHolder.ItemLongClicked -= NotificationAdapterViewHolder_ItemLongClicked;
            CatcherHelper.NotificationRemoved -= CatcherHelper_NotificationRemoved;
            CatcherHelper.NotificationPosted -= CatcherHelper_NotificationPosted;
            if (floatingNotificationView != null)
            {
                floatingNotificationAppName.Dispose();
                floatingNotificationWhen.Dispose();
                floatingNotificationTitle.Dispose();
                floatingNotificationText.Dispose();
                windowManager.RemoveView(floatingNotificationView);
                windowManager.Dispose();
            }
            openNotification?.Dispose();
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            if (e.Action == MotionEventActions.Outside)
            {
                floatingNotificationView.Visibility = ViewStates.Invisible;
            }
            else if (e.Action == MotionEventActions.Up)
            {
                openNotification.ClickNotification();
                floatingNotificationView.Visibility = ViewStates.Gone;
            }
            return true;
        }
    }
}
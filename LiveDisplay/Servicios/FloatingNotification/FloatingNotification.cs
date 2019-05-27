using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using System;

namespace LiveDisplay.Servicios.FloatingNotification
{
    //Will spawn a View to show the clicked notification in the list, while the music is playing.
    [Service(Enabled = true)]
    internal class FloatingNotification : Service
    {
        private IWindowManager windowManager;
        private LinearLayout floatingNotificationView;
        private TextView floatingNotificationTitle;
        private TextView floatingNotificationText;
        private TextView floatingNotificationAppName;
        private TextView floatingNotificationWhen;
        private LinearLayout floatingNotificationActionsContainer;
        private int position; //Represents the notification position in the NotificationList.

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        //TODO: Create a touchListener, and if the touch registered by this touchlistener is outside the bounds of the
        //actual floating notification the same will be hidden, this change is more of UX.
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
                Flags = WindowManagerFlags.NotFocusable,
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

            CatcherHelper.NotificationUpdated += CatcherHelper_NotificationUpdated;
            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            NotificationAdapterViewHolder.ItemClicked += NotificationAdapterViewHolder_ItemClicked;
            NotificationAdapterViewHolder.ItemLongClicked += NotificationAdapterViewHolder_ItemLongClicked;
            LockScreenActivity.OnActivityStateChanged += LockScreenActivity_OnActivityStateChanged;
            floatingNotificationView.Click += FloatingNotificationView_Click;
        }

        private void LockScreenActivity_OnActivityStateChanged(object sender, Activities.ActivitiesEventArgs.LockScreenLifecycleEventArgs e)
        {
            switch (e.State)
            {
                case Activities.ActivityStates.Paused:
                    if (floatingNotificationView.Visibility == ViewStates.Visible)
                        floatingNotificationView.Visibility = ViewStates.Invisible;
                    break;
                case Activities.ActivityStates.Resumed:
                    //?
                    break;
                case Activities.ActivityStates.Destroyed:
                    if (floatingNotificationView.Visibility == ViewStates.Visible)
                        floatingNotificationView.Visibility = ViewStates.Invisible;
                    break;
                default:
                    break;
            }
        }

        private void CatcherHelper_NotificationPosted(object sender, NotificationPostedEventArgs e)
        {
            if (e.ShouldCauseWakeUp == true)
            {
                Awake.WakeUpScreen();
            }
        }

        private void CatcherHelper_NotificationRemoved(object sender, EventArgs e)
        {
            floatingNotificationView.Visibility = ViewStates.Gone;

        }

        private void CatcherHelper_NotificationUpdated(object sender, NotificationItemClickedEventArgs e)
        {
            if (position > -1) //Avoid out of bounds exceptions, i guess, the out of bounds vaue is -1, i guess, too.
                using (OpenNotification openNotification = new OpenNotification(e.Position))
                {
                    floatingNotificationAppName.Text = openNotification.AppName();
                    floatingNotificationWhen.Text = openNotification.When();
                    floatingNotificationTitle.Text = openNotification.Title();
                    floatingNotificationText.Text = openNotification.Text();
                    floatingNotificationActionsContainer.RemoveAllViews();

                    if (openNotification.HasActionButtons() == true)
                    {
                        var actions = openNotification.RetrieveActions();
                        foreach (var a in actions)
                        {
                            OpenAction openAction = new OpenAction(a);
                            float weight = (float)1 / actions.Count;

                            Button anActionButton = new Button(Application.Context)
                            {
                                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent, weight),
                                Text = openAction.GetTitle(),
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

        private void NotificationAdapterViewHolder_ItemLongClicked(object sender, NotificationItemClickedEventArgs e)
        {
            position = e.Position;
            if (position > 0) //Avoid out of bounds exceptions, i guess, the out of bounds vaue is -1, i guess, too.
                using (OpenNotification openNotification = new OpenNotification(e.Position))
                {
                    if (openNotification.IsRemovable())
                    {
                        using (NotificationSlave notificationSlave = NotificationSlave.NotificationSlaveInstance())
                        {
                            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                            {
                                int notiId = CatcherHelper.statusBarNotifications[position].Id;
                                string notiTag = CatcherHelper.statusBarNotifications[position].Tag;
                                string notiPack = CatcherHelper.statusBarNotifications[position].PackageName;
                                notificationSlave.CancelNotification(notiPack, notiTag, notiId);
                            }
                            else
                            {
                                notificationSlave.CancelNotification(CatcherHelper.statusBarNotifications[position].Key);
                            }
                        }
                        floatingNotificationView.Visibility = ViewStates.Gone;
                    }
                }
        }

        private void NotificationAdapterViewHolder_ItemClicked(object sender, NotificationItemClickedEventArgs e)
        {
            position = e.Position;
            if (position > -1) //Avoid out of bounds exceptions, i guess, the out of bounds vaue is -1, i guess, too.
                using (OpenNotification openNotification = new OpenNotification(e.Position))
                {
                    floatingNotificationAppName.Text = openNotification.AppName();
                    floatingNotificationWhen.Text = openNotification.When();
                    floatingNotificationTitle.Text = openNotification.Title();
                    floatingNotificationText.Text = openNotification.Text();
                    floatingNotificationActionsContainer.RemoveAllViews();

                    if (openNotification.HasActionButtons() == true)
                    {
                        var actions = openNotification.RetrieveActions();
                        foreach (var a in actions)
                        {
                            OpenAction openAction = new OpenAction(a);
                            float weight = (float)1 / actions.Count;

                            Button anActionButton = new Button(Application.Context)
                            {
                                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent, weight),
                                Text = openAction.GetTitle(),
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
            else
            {
                floatingNotificationView.Visibility = ViewStates.Invisible;
            }
        }

        private void FloatingNotificationView_Click(object sender, EventArgs e)
        {
            if (position > -1) //Avoid out of bounds exceptions, i guess, the out of bounds vaue is -1, i guess, too.
                using (OpenNotification openNotification = new OpenNotification(position))
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
            CatcherHelper.NotificationUpdated -= CatcherHelper_NotificationUpdated;
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
        }
    }
}
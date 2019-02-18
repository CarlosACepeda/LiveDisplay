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

            NotificationAdapterViewHolder.ItemClicked += NotificationAdapterViewHolder_ItemClicked;
            NotificationAdapterViewHolder.ItemLongClicked += NotificationAdapterViewHolder_ItemLongClicked;

            floatingNotificationView.Click += FloatingNotificationView_Click;
        }

        private void NotificationAdapterViewHolder_ItemLongClicked(object sender, Notificaciones.NotificationEventArgs.NotificationItemClickedEventArgs e)
        {
            position = e.Position;
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

        private void NotificationAdapterViewHolder_ItemClicked(object sender, Notificaciones.NotificationEventArgs.NotificationItemClickedEventArgs e)
        {
            position = e.Position;
            using (OpenNotification notification = new OpenNotification(e.Position))
            {
                position = e.Position;
                using (OpenNotification openNotification = new OpenNotification(e.Position))
                {
                    floatingNotificationAppName.Text = notification.GetAppName();
                    floatingNotificationWhen.Text = notification.GetWhen();
                    floatingNotificationTitle.Text = notification.GetTitle();
                    floatingNotificationText.Text = notification.GetText();
                    floatingNotificationActionsContainer.RemoveAllViews();

                    if (openNotification.NotificationHasActionButtons() == true)
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
                            anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
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
            };
        }

        private void FloatingNotificationView_Click(object sender, EventArgs e)
        {
            OpenNotification.ClickNotification(position);
            floatingNotificationView.Visibility = ViewStates.Gone;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            floatingNotificationView.Click -= FloatingNotificationView_Click;
            NotificationAdapterViewHolder.ItemClicked -= NotificationAdapterViewHolder_ItemClicked;
            NotificationAdapterViewHolder.ItemLongClicked -= NotificationAdapterViewHolder_ItemLongClicked;
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
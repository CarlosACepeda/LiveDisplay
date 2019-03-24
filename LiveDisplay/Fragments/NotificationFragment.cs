using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using LiveDisplay.Servicios.Wallpaper;
using System;
using System.Threading;

namespace LiveDisplay.Fragments
{
    public class NotificationFragment : Fragment
    {
        public static event EventHandler NotificationClicked;

        private int position;
        private LinearLayout notificationActions;
        private TextView titulo;
        private TextView texto;
        private TextView appName;
        private TextView when;
        private LinearLayout notification;
        private ImageButton closenotificationbutton;
        private bool timeoutStarted = false;

        #region Lifecycle events

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.NotificationFrag, container, false);

            notificationActions = v.FindViewById<LinearLayout>(Resource.Id.notificationActions);
            texto = v.FindViewById<TextView>(Resource.Id.tvTexto);
            titulo = v.FindViewById<TextView>(Resource.Id.tvTitulo);
            when = v.FindViewById<TextView>(Resource.Id.tvWhen);
            appName = v.FindViewById<TextView>(Resource.Id.tvAppName);
            notification = v.FindViewById<LinearLayout>(Resource.Id.llNotification);
            closenotificationbutton = v.FindViewById<ImageButton>(Resource.Id.closenotificationbutton);
            //Subscribe to events raised by several types.
            notification.Drag += Notification_Drag;
            notification.Click += LlNotification_Click;
            closenotificationbutton.Click += Closenotificationbutton_Click;
            NotificationAdapterViewHolder.ItemClicked += ItemClicked;
            NotificationAdapterViewHolder.ItemLongClicked += ItemLongClicked;
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationUpdated += CatcherHelper_NotificationUpdated;
            CatcherHelper.NotificationRemoved += CatcherHelper_NotificationRemoved;
            return v;
        }

        private void Closenotificationbutton_Click(object sender, EventArgs e)
        {
            using (OpenNotification openNotification = new OpenNotification(position))
            {
                if (openNotification.IsRemovable())
                {
                    using (NotificationSlave slave = NotificationSlave.NotificationSlaveInstance())
                    {
                        if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                        {
                            int notiId = CatcherHelper.statusBarNotifications[position].Id;
                            string notiTag = CatcherHelper.statusBarNotifications[position].Tag;
                            string notiPack = CatcherHelper.statusBarNotifications[position].PackageName;
                            slave.CancelNotification(notiPack, notiTag, notiId);
                        }
                        else
                        {
                            slave.CancelNotification(CatcherHelper.statusBarNotifications[position].Key);
                        }
                    }
                    notification.Visibility = ViewStates.Invisible;
                    titulo.Text = null;
                    texto.Text = null;
                    notificationActions.RemoveAllViews();
                }
            }

        }

        private void Notification_Drag(object sender, View.DragEventArgs e)
        {
            StartTimeout(); //To keep the notification visible while the user touches the notification fragment
        }

        private void CatcherHelper_NotificationPosted(object sender, NotificationPostedEventArgs e)
        {
            if (e.ShouldCauseWakeUp == true)
            {
                Awake.WakeUpScreen();
            }
        }

        public override void OnDestroy()
        {
            NotificationAdapterViewHolder.ItemClicked -= ItemClicked;
            NotificationAdapterViewHolder.ItemLongClicked -= ItemLongClicked;
            CatcherHelper.NotificationUpdated -= CatcherHelper_NotificationUpdated;
            CatcherHelper.NotificationRemoved -= CatcherHelper_NotificationRemoved;
            CatcherHelper.NotificationPosted -= CatcherHelper_NotificationPosted;
            notificationActions.Dispose();
            texto.Dispose();
            titulo.Dispose();
            when.Dispose();
            appName.Dispose();
            notification.Dispose();
            closenotificationbutton.Dispose();
            base.OnDestroy();
        }

        #endregion Lifecycle events

        #region Events Implementation:

        private void CatcherHelper_NotificationRemoved(object sender, EventArgs e)
        {
            notification.Visibility = ViewStates.Gone;
        }

        private void CatcherHelper_NotificationUpdated(object sender, NotificationItemClickedEventArgs e)
        {
            ItemClicked(this, e);
        }

        private void LlNotification_Click(object sender, EventArgs e)
        {
            notification.Visibility = ViewStates.Visible;
            try
            {
                using (OpenNotification openNotification = new OpenNotification(position))
                {
                    Activity.RunOnUiThread(() =>
                    openNotification.ClickNotification()
                    );
                    if (OpenNotification.NotificationIsAutoCancel(position) == true)
                    {
                        notification.Visibility = ViewStates.Invisible;
                        titulo.Text = null;
                        texto.Text = null;
                        when.Text = null;
                        notificationActions.RemoveAllViews();
                    }
                }
            }
            catch
            {
                Log.Wtf("OnNotificationClicked", "Metodo falla porque no existe una notificacion con esta acción");
            }
        }

        private void ItemLongClicked(object sender, NotificationItemClickedEventArgs e)
        {
            position = e.Position;
            notification.Visibility = ViewStates.Visible;
            using (OpenNotification openNotification = new OpenNotification(e.Position))
            {
                //If the notification is removable...
                if (openNotification.IsRemovable())
                {
                    //Then remove the notification
                    using (NotificationSlave slave = NotificationSlave.NotificationSlaveInstance())
                    {
                        if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                        {
                            int notiId = CatcherHelper.statusBarNotifications[position].Id;
                            string notiTag = CatcherHelper.statusBarNotifications[position].Tag;
                            string notiPack = CatcherHelper.statusBarNotifications[position].PackageName;
                            slave.CancelNotification(notiPack, notiTag, notiId);
                        }
                        else
                        {
                            slave.CancelNotification(CatcherHelper.statusBarNotifications[position].Key);
                        }
                    }
                    notification.Visibility = ViewStates.Invisible;
                    titulo.Text = null;
                    texto.Text = null;
                    notificationActions.RemoveAllViews();
                }
            }
        }

        private void ItemClicked(object sender, NotificationItemClickedEventArgs e)
        {
            position = e.Position;
            using (OpenNotification openNotification = new OpenNotification(e.Position))
            {
                //ThreadPool.QueueUserWorkItem(method =>
                //{
                //    var notificationBigPicture = new BitmapDrawable(Resources, openNotification.GetBigPicture());
                //    WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs { Wallpaper = notificationBigPicture, OpacityLevel = 125, SecondsOfAttention = 5 });
                //});
                titulo.Text = openNotification.GetTitle();
                texto.Text = openNotification.GetText();
                appName.Text = openNotification.GetAppName();
                when.Text = openNotification.GetWhen();
                notificationActions.RemoveAllViews();

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
                            Text = openAction.GetTitle()
                        };
                        anActionButton.TransformationMethod = null;
                        anActionButton.SetTypeface(Typeface.Create("sans-serif-condensed", TypefaceStyle.Normal), TypefaceStyle.Normal);
                        anActionButton.SetMaxLines(1);
                        anActionButton.SetTextColor(Color.White);
                        anActionButton.Click += (o, eventargs) =>
                         {
                             openAction.ClickAction();
                         };
                        anActionButton.Gravity = GravityFlags.CenterVertical;
                        TypedValue outValue = new TypedValue();
                        Application.Context.Theme.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackgroundBorderless, outValue, true);
                        anActionButton.SetBackgroundResource(outValue.ResourceId);
                        anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                        notificationActions.AddView(anActionButton);
                    };
                }
                if (openNotification.IsRemovable())
                {
                    closenotificationbutton.Visibility = ViewStates.Visible;
                }
                else
                {
                    closenotificationbutton.Visibility = ViewStates.Invisible;
                }
            }
            if (notification.Visibility != ViewStates.Visible)
            {
                notification.Visibility = ViewStates.Visible;
                StartTimeout();
            }

            NotificationClicked?.Invoke(null, EventArgs.Empty);
        }

        #endregion Events Implementation:

        //THis works like a charm :)
        private void StartTimeout()
        {
            //This action is: 'Hide the notification, and set the timeoutStarted as finished(false)
            //because this action will be invoked only when the timeout has finished.
            Action hideNotification = () => { notification.Visibility = ViewStates.Gone; timeoutStarted = false; };
            //If the timeout has started, then cancel the action, and start again.
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
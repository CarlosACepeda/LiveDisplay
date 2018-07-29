using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Notificaciones;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
using System;

namespace LiveDisplay.Fragments
{
    public class NotificationFragment : Fragment
    {
        private int position;
        private LinearLayout notificationActions;
        private TextView titulo;
        private TextView texto;
        private TextView appName;
        private TextView when;
        private LinearLayout notification;

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

            //Subscribe to events raised by several types.
            notification.Click += LlNotification_Click;
            NotificationAdapterViewHolder.ItemClicked += ItemClicked;
            NotificationAdapterViewHolder.ItemLongClicked += ItemLongClicked;
            return v;
        }

        private void LlNotification_Click(object sender, EventArgs e)
        {
            notification.Visibility = ViewStates.Visible;
            try
            {
                Activity.RunOnUiThread(() =>
                OpenNotification.ClickNotification(position)
                );
                if (OpenNotification.NotificationIsAutoCancel(position) == true)
                {
                    notification.Visibility = ViewStates.Invisible;
                    titulo.Text = null;
                    texto.Text = null;
                    notificationActions = null;
                }
            }
            catch
            {
                Log.Wtf("OnNotificationClicked", "Metodo falla porque no existe una notificacion con esta acción");
            }
        }

        private void ItemLongClicked(object sender, NotificationItemClickedEventArgs e)
        {
            notification.Visibility = ViewStates.Visible;
            //If the notification is removable...
            if (OpenNotification.IsRemovable(e.Position) == true)
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

        private void ItemClicked(object sender, NotificationItemClickedEventArgs e)
        {
            position = e.Position;
            //When an item of the list is clicked, then fill A notification with the position of the item.
            using (OpenNotification notification = new OpenNotification(e.Position))
            {
                titulo.Text = notification.GetTitle();
                texto.Text = notification.GetText();
                appName.Text = notification.GetAppName();
                //Fix me:
                //when.Text = notification.GetWhen();
                notificationActions.RemoveAllViews();
                notificationActions.WeightSum = 1f;

                if (OpenNotification.NotificationHasActionButtons(e.Position) == true)
                {
                    foreach (var a in OpenNotification.RetrieveActionButtons(e.Position))
                    {
                        notificationActions.AddView(a);
                    }
                }
            }
            if (notification.Visibility != ViewStates.Visible)
            {
                notification.Visibility = ViewStates.Visible;
            }
        }
    }
}
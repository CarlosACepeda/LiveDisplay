using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Servicios.Notificaciones;
using System;

namespace LiveDisplay.Fragments
{
    public class NotificationFragment : Fragment
    {
        private int position;
        LinearLayout notificationActions;
        TextView tvTitulo;
        TextView tvTexto;
        LinearLayout llNotification;
        LockScreenActivity lockScreenActivity;

        public override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);
            
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            using (View v = inflater.Inflate(Resource.Layout.NotificationFrag, container, false))
            {
                notificationActions = v.FindViewById<LinearLayout>(Resource.Id.notificationActions);
                tvTexto = v.FindViewById<TextView>(Resource.Id.tvTexto);
                tvTitulo = v.FindViewById<TextView>(Resource.Id.tvTitulo);
                llNotification = v.FindViewById<LinearLayout>(Resource.Id.llNotification);

                llNotification.Click += LlNotification_Click;
                lockScreenActivity = (LockScreenActivity)Activity;
                SubscribeToEvents();
                return v;
            }
        }

        private void LlNotification_Click(object sender, EventArgs e)
        {
            try
            {
                Activity.RunOnUiThread(() =>
                OpenNotification.ClickNotification(position)
                );
                //TODO: Manage this In Fragment
                if (OpenNotification.NotificationIsAutoCancel(position) == true)
                {
                    llNotification.Visibility = ViewStates.Invisible;
                }
                llNotification.Visibility = ViewStates.Visible;

            }
            catch
            {
                Log.Wtf("OnNotificationClicked", "Metodo falla porque no existe una notificacion con esta acción");
            }
        }

        private void SubscribeToEvents()
        {
            lockScreenActivity.NotificationItemClicked += LockScreenInstance_NotificationItemClicked;
        }

        private void LockScreenInstance_NotificationItemClicked(object sender, Servicios.Notificaciones.NotificationEventArgs.NotificationItemClickedEventArgs e)
        {
            ///This fragment starts invisible.
            if (llNotification.Visibility != ViewStates.Visible)
            {
                llNotification.Visibility = ViewStates.Visible;
            }
            position = e.Position;
            //Define events to communicate with the Notification Widget:
            //When this method is called, tell NotificationWidget to update itself with data provided
            //by this method.

            using (OpenNotification notification = new OpenNotification(e.Position))
            {
                tvTitulo.Text = notification.GetTitle();
                tvTexto.Text = notification.GetText();
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

        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            lockScreenActivity.NotificationItemClicked -= LockScreenInstance_NotificationItemClicked;
            notificationActions.Dispose();
            tvTexto.Dispose();
            tvTitulo.Dispose();
            llNotification.Dispose();

        }
    }
}
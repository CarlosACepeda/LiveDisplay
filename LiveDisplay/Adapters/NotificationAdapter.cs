namespace LiveDisplay.Adapters
{
    using Android.App;
    using Android.OS;
    using Android.Service.Notification;
    using Android.Util;
    using Android.Views;
    using Android.Widget;
    using AndroidX.RecyclerView.Widget;
    using LiveDisplay.Factories;
    using LiveDisplay.Servicios.Notificaciones;
    using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
    using System;
    using System.Collections.Generic;

    public class NotificationAdapter : RecyclerView.Adapter
    {
        public static int selectedItem = -1;
        public List<OpenNotification> notifications = new List<OpenNotification>();
        public override int ItemCount => notifications.Count;

        public NotificationAdapter(List<OpenNotification> notificaciones)
        {
            this.notifications = notificaciones;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (position != RecyclerView.NoPosition)
            {
                //Cast
                NotificationAdapterViewHolder viewHolder = holder as NotificationAdapterViewHolder;

                viewHolder.Icono.Background = notifications[position].GetSmallIcon().LoadDrawable(Application.Context);
                if (selectedItem == position)
                {
                    viewHolder.Icono.Alpha = 0.5f;
                }
                else
                {
                    viewHolder.Icono.Alpha = 1;
                }
            }
            else
            {
                Log.Info("LiveDisplay", "WTF Position: " + position);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(parent.Context);
            View itemView = layoutInflater.Inflate(Resource.Layout.NotificationItemRow, parent, false);
            return new NotificationAdapterViewHolder(itemView);
        }
    }

    //The following class just simply saves the view's references to the row, in order to avoid making calls to 'FindViewById' each time, nothing more is done here.
    internal class NotificationAdapterViewHolder : RecyclerView.ViewHolder
    {
        public static event EventHandler<NotificationItemClickedEventArgs> ItemClicked;

        public static event EventHandler<NotificationItemClickedEventArgs> ItemLongClicked;

        public ImageView Icono { get; set; }
        public OpenNotification OpenNotification { get; set; }

        public NotificationAdapterViewHolder(View itemView) : base(itemView)
        {
            Icono = itemView.FindViewById<ImageView>(Resource.Id.ivNotificationIcon);
            itemView.Click += ItemView_Click;
            itemView.LongClick += ItemView_LongClick;
        }

        private void ItemView_LongClick(object sender, View.LongClickEventArgs e)
        {
            var statusBarNotification = CatcherHelper.OpenNotifications[LayoutPosition];
            OnItemLongClicked(LayoutPosition, statusBarNotification);
        }

        private void ItemView_Click(object sender, EventArgs e)
        {
            //Simply indicates which item was clicked and after that call NotifyDataSetChanged to changes take effect.
            NotificationAdapter.selectedItem = LayoutPosition;
            //CatcherHelper.notificationAdapter.NotifyDataSetChanged();
            var statusBarNotification = CatcherHelper.OpenNotifications[LayoutPosition];
            OnItemClicked(LayoutPosition, statusBarNotification);
            //try
            //{
            //    var view = sender as View;
            //    view.Visibility = ViewStates.Gone;
            //}
            //catch (Exception ex)
            //{
            //    Log.Info("LiveDisplay", "Exception hiding notification" + ex.Message);
            //}
        }

        private void OnItemClicked(int position, OpenNotification sbn)
        {
            ItemClicked?.Invoke(this, new NotificationItemClickedEventArgs
            {
                Position = position,
                OpenNotification = sbn
            });
        }

        private void OnItemLongClicked(int position, OpenNotification sbn)
        {
            ItemLongClicked?.Invoke(this, new NotificationItemClickedEventArgs
            {
                Position = position,
                OpenNotification = sbn
            }
            );
        }
    }
}
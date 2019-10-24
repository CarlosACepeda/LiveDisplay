﻿namespace LiveDisplay.Adapters
{
    using Android.Service.Notification;
    using Android.Support.V7.Widget;
    using Android.Views;
    using Android.Widget;
    using LiveDisplay.Factories;
    using LiveDisplay.Servicios.Notificaciones;
    using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;
    using System;
    using System.Collections.Generic;

    public class NotificationAdapter : RecyclerView.Adapter
    {
        public static int selectedItem = -1;
        public List<StatusBarNotification> notificaciones = new List<StatusBarNotification>();

        public override int ItemCount => notificaciones.Count;

        public NotificationAdapter(List<StatusBarNotification> notificaciones)
        {
            this.notificaciones = notificaciones;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            //Cast
            NotificationAdapterViewHolder viewHolder = holder as NotificationAdapterViewHolder;
            viewHolder.Icono.Background = IconFactory.ReturnIconDrawable(notificaciones[position].Notification.Icon, notificaciones[position].PackageName);
            if (selectedItem == position)
            {
                viewHolder.Icono.Alpha = 0.5f;
            }
            else
            {
                viewHolder.Icono.Alpha = 1;
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(parent.Context);
            View itemView = layoutInflater.Inflate(Resource.Layout.NotificationItemRow, parent, false);
            return new NotificationAdapterViewHolder(itemView);
        }
    }

    //La siguiente clase simplemente guarda referencias a las vistas de la fila, para evitar hacer llamadas a FindViewById cada vez, no se hace nada más aquí
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
            var statusBarNotification = CatcherHelper.StatusBarNotifications[LayoutPosition];
            OnItemLongClicked(LayoutPosition, statusBarNotification);
        }

        private void ItemView_Click(object sender, EventArgs e)
        {
            //Simply indicates which item was clicked and after that call NotifyDataSetChanged to changes take effect.
            NotificationAdapter.selectedItem = LayoutPosition;
            CatcherHelper.notificationAdapter.NotifyDataSetChanged();
            var statusBarNotification = CatcherHelper.StatusBarNotifications[LayoutPosition];
            OnItemClicked(LayoutPosition, statusBarNotification);
        }

        private void OnItemClicked(int position, StatusBarNotification sbn)
        {
            ItemClicked?.Invoke(this, new NotificationItemClickedEventArgs
            {
                Position = position,
                StatusBarNotification= sbn

            });
        }

        private void OnItemLongClicked(int position, StatusBarNotification sbn)
        {
            ItemLongClicked?.Invoke(this, new NotificationItemClickedEventArgs
            {
                Position = position,
                StatusBarNotification= sbn
            }
            );
        }
    }
}
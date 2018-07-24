using Android.Service.Notification;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using LiveDisplay.Factories;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Notificaciones;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Adapters
{
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
            NotificationAdapterViewHolder viewHolder = holder as NotificationAdapterViewHolder;
            viewHolder.Icono.Background = IconFactory.ReturnIconDrawable(notificaciones[position].Notification.Icon, notificaciones[position].PackageName); 
            //FUnciona, habrá otra forma más simple?
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
        private delegate void ItemOnClickListener(int position);

        private delegate void ItemOnLongClickListener(int position);

        public ImageView Icono { get; set; }


        public NotificationAdapterViewHolder(View itemView) : base(itemView)
        {
            Icono = itemView.FindViewById<ImageView>(Resource.Id.ivNotificationIcon);
            itemView.Click += ItemView_Click;
            itemView.LongClick += ItemView_LongClick;
        }

        //TODO: Invoke these events on the Fragment? Don't use activity for this callbacks?
        private void ItemView_LongClick(object sender, View.LongClickEventArgs e)
        {
            //Invoke a Event instead //new ItemOnLongClickListener(LockScreenActivity.lockScreenInstance.OnItemLongClick).Invoke(LayoutPosition);
        }

        private void ItemView_Click(object sender, EventArgs e)
        {
            NotificationAdapter.selectedItem = LayoutPosition;
            CatcherHelper.notificationAdapter.NotifyDataSetChanged();

            //Invoke an event Instead

            //new ItemOnClickListener(LockScreenActivity.lockScreenInstance.OnItemClick).Invoke(LayoutPosition);
        }
    }
}
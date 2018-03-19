using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Service.Notification;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Adapters
{
    public class NotificationAdapter : RecyclerView.Adapter
    {

        public List<StatusBarNotification> notificaciones = new List<StatusBarNotification>();

        public override int ItemCount => notificaciones.Count;


        public NotificationAdapter(List<StatusBarNotification> notificaciones)
        {
            this.notificaciones = notificaciones;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            NotificationAdapterViewHolder viewHolder = holder as NotificationAdapterViewHolder;
            //Arréglame
            //Funciona en Kitkat?
            viewHolder.Icono.Background = (ReturnIconBitMap(notificaciones[position].Notification.Icon, notificaciones[position].PackageName));
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(parent.Context);
            View itemView = layoutInflater.Inflate(Resource.Layout.NotificationItemRow, parent, false);
            return new NotificationAdapterViewHolder(itemView);
        }

        public Drawable ReturnIconBitMap(int iconInt, string paquete)
        {
            Context remotePackageContext = null;
            remotePackageContext = Application.Context.CreatePackageContext(paquete, 0);
            Drawable icon = ContextCompat.GetDrawable(remotePackageContext, iconInt);
            return icon;
        }
    }

    //La siguiente clase simplemente guarda referencias a las vistas de la fila, para evitar hacer llamadas a FindViewById cada vez, no se hace nada más aquí
    internal class NotificationAdapterViewHolder : RecyclerView.ViewHolder
    {
        private delegate void ItemOnClickListener(int position);
        private delegate void ItemOnLongClickListener(int position);
        public CardView Icono { get; set; }


        public NotificationAdapterViewHolder(View itemView) : base(itemView)
        {
            Icono = itemView.FindViewById<CardView>(Resource.Id.cvNotificationIcon);
            itemView.Click += ItemView_Click;
            itemView.LongClick += ItemView_LongClick;
        }

        private void ItemView_LongClick(object sender, View.LongClickEventArgs e)
        {
            new ItemOnLongClickListener(LockScreenActivity.lockScreenInstance.OnItemLongClick).Invoke(LayoutPosition);
        }

        private void ItemView_Click(object sender, EventArgs e)
        {
            new ItemOnClickListener(LockScreenActivity.lockScreenInstance.OnItemClick).Invoke(LayoutPosition);
        }
    }
}
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using LiveDisplay.Objects;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Adapters
{
    public class NotificationAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public List<ClsNotification> notificaciones = new List<ClsNotification>();

        public override int ItemCount => notificaciones.Count;

        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        public NotificationAdapter(List<ClsNotification> notificaciones)
        {
            this.notificaciones = notificaciones;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            NotificationAdapterViewHolder viewHolder = holder as NotificationAdapterViewHolder;
            viewHolder.Icono.Background = (ReturnIconBitMap(notificaciones[position].Icono, notificaciones[position].Paquete));
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(parent.Context);
            View itemView = layoutInflater.Inflate(Resource.Layout.NotificationItemRow, parent, false);
            return new NotificationAdapterViewHolder(itemView, OnClick);
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
        public CardView Icono { get; set; }

        public NotificationAdapterViewHolder(View itemView, Action<int> listener) : base(itemView)
        {
            Icono = itemView.FindViewById<CardView>(Resource.Id.cvNotificationIcon);
            itemView.Click += (sender, e) => listener(base.LayoutPosition);
        }

    }
}
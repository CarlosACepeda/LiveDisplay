using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using LiveDisplay.Objects;
using Android.App;
using System.Collections.Generic;
using Android.Graphics;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.OS;

namespace LiveDisplay.Adapters
{
    class NotificationAdapter : RecyclerView.Adapter
    {
        public List<ClsNotification> notificaciones = new List<ClsNotification>();

        public override int ItemCount => notificaciones.Count;

        public NotificationAdapter(List<ClsNotification> notificaciones)
        {
            this.notificaciones = notificaciones;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            NotificationAdapterViewHolder viewHolder = holder as NotificationAdapterViewHolder;
            viewHolder.Icono.Background=(ReturnIconBitMap(notificaciones[position].Icono, notificaciones[position].Paquete));
            viewHolder.Titulo.Text = notificaciones[position].Titulo;
            viewHolder.Texto.Text = notificaciones[position].Texto;
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
    class NotificationAdapterViewHolder : RecyclerView.ViewHolder
    {
        public CardView Icono { get; set; }
        public TextView Titulo { get; set; }
        public TextView Texto { get; set; }

        public NotificationAdapterViewHolder(View itemView) : base(itemView)
        {
            Icono = (CardView)itemView.FindViewById(Resource.Id.cvNotificationIcon);
            Titulo = (TextView)itemView.FindViewById(Resource.Id.tvNotificationTitle);
            Texto = (TextView)itemView.FindViewById(Resource.Id.tvNotificationText);
        }
    }
}
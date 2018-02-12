using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using LiveDisplay.Objects;
using Android.App;
using System.Collections.Generic;
using Android.Graphics;

namespace LiveDisplay.Adapters
{
    class NotificationAdapter : RecyclerView.Adapter
    {
        private List<ClsNotification> notificaciones = new List<ClsNotification>();

        public override int ItemCount => notificaciones.Count;

        public NotificationAdapter(List<ClsNotification> notificaciones)
        {
            this.notificaciones = notificaciones;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            //Bitmap bitmap = ReturnIconBitMap(notificaciones[position].Icono);
            NotificationAdapterViewHolder viewHolder = holder as NotificationAdapterViewHolder;
            viewHolder.Icono.SetImageResource(Resource.Drawable.ic_preview_lockscreen);
            viewHolder.Titulo.Text = notificaciones[position].Titulo;
            viewHolder.Texto.Text = notificaciones[position].Texto;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(parent.Context);
            View itemView = layoutInflater.Inflate(Resource.Layout.NotificationItemRow, parent, false);
            return new NotificationAdapterViewHolder(itemView);
        }
        //todo
        public Bitmap ReturnIconBitMap(byte[] iconByteArray)
        {
            return BitmapFactory.DecodeByteArray(iconByteArray, 0, iconByteArray.Length);
        }
    }

    //La siguiente clase simplemente guarda referencias a las vistas de la fila, para evitar hacer llamadas a FindViewById cada vez, no se hace nada más aquí
    class NotificationAdapterViewHolder : RecyclerView.ViewHolder
    {
        public ImageView Icono { get; set; }
        public TextView Titulo { get; set; }
        public TextView Texto { get; set; }

        public NotificationAdapterViewHolder(View itemView) : base(itemView)
        {
            Icono = (ImageView)itemView.FindViewById(Resource.Id.ivNotificationIcon);
            Titulo = (TextView)itemView.FindViewById(Resource.Id.tvNotificationTitle);
            Texto = (TextView)itemView.FindViewById(Resource.Id.tvNotificationText);
        }
    }

}
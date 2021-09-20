using Android.Views;
using AndroidX.RecyclerView.Widget;
using LiveDisplay.Services.Notifications;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Adapters
{
    internal class FilteredNotificationAdapter : RecyclerView.Adapter
    {
        public event EventHandler<FilteredNotificationAdapterClickEventArgs> ItemClick;

        public event EventHandler<FilteredNotificationAdapterClickEventArgs> ItemLongClick;

        private List<OpenNotification> filteredOpenNotifications;

        public FilteredNotificationAdapter(List<OpenNotification> filteredOpenNotifications)
        {
            this.filteredOpenNotifications = filteredOpenNotifications;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            //Setup your layout here
            View itemView = null;
            //var id = Resource.Layout.__YOUR_ITEM_HERE;
            //itemView = LayoutInflater.From(parent.Context).
            //       Inflate(id, parent, false);

            var vh = new FilteredNotificationAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = filteredOpenNotifications[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as FilteredNotificationAdapterViewHolder;
            //holder.TextView.Text = items[position];
        }

        public override int ItemCount => filteredOpenNotifications.Count;

        private void OnClick(FilteredNotificationAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);

        private void OnLongClick(FilteredNotificationAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
    }

    public class FilteredNotificationAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }

        public FilteredNotificationAdapterViewHolder(View itemView, Action<FilteredNotificationAdapterClickEventArgs> clickListener,
                            Action<FilteredNotificationAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView = v;
            itemView.Click += (sender, e) => clickListener(new FilteredNotificationAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new FilteredNotificationAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class FilteredNotificationAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}
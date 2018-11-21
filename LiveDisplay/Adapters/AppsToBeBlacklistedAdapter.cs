using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace LiveDisplay.Adapters
{
    class AppsToBeBlacklistedAdapter : RecyclerView.Adapter
    {
        public event EventHandler<AppsToBeBlacklistedAdapterClickEventArgs> ItemClick;
        public event EventHandler<AppsToBeBlacklistedAdapterClickEventArgs> ItemLongClick;
        string[] items;

        public AppsToBeBlacklistedAdapter(string[] data)
        {
            items = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = null;
            //var id = Resource.Layout.__YOUR_ITEM_HERE;
            //itemView = LayoutInflater.From(parent.Context).
            //       Inflate(id, parent, false);

            var vh = new AppsToBeBlacklistedAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = items[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as AppsToBeBlacklistedAdapterViewHolder;
            //holder.TextView.Text = items[position];
        }

        public override int ItemCount => items.Length;

        void OnClick(AppsToBeBlacklistedAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(AppsToBeBlacklistedAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class AppsToBeBlacklistedAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }


        public AppsToBeBlacklistedAdapterViewHolder(View itemView, Action<AppsToBeBlacklistedAdapterClickEventArgs> clickListener,
                            Action<AppsToBeBlacklistedAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView = v;
            itemView.Click += (sender, e) => clickListener(new AppsToBeBlacklistedAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new AppsToBeBlacklistedAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class AppsToBeBlacklistedAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}
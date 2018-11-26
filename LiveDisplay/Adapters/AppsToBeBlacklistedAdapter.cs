using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Android.Content.PM;
using LiveDisplay.Servicios.Blacklist;
using LiveDisplay.Misc;

namespace LiveDisplay.Adapters
{
    class AppsToBeBlacklistedAdapter : RecyclerView.Adapter
    {
        public event EventHandler<AppsToBeBlacklistedAdapterClickEventArgs> ItemClick;
        public event EventHandler<AppsToBeBlacklistedAdapterClickEventArgs> ItemLongClick;
        List<PackageInfo> items;

        public AppsToBeBlacklistedAdapter(List<PackageInfo> listofapppackages)
        {
            items = listofapppackages;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            LayoutInflater layoutInflater = LayoutInflater.From(parent.Context);
            View itemView = layoutInflater.Inflate(Resource.Layout.BlacklistItemRow, parent, false);
            return new AppsToBeBlacklistedAdapterViewHolder(itemView, OnClick, OnLongClick);
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {

            // Replace the contents of the view with that element
            var holder = viewHolder as AppsToBeBlacklistedAdapterViewHolder;
            holder.App.Text = PackageUtils.GetTheAppName(items[position].PackageName);
            holder.blacklistToggle.Checked = Blacklist.IsAppBlacklisted(items[position].PackageName);
        }

        public override int ItemCount => items.Count;

        void OnClick(AppsToBeBlacklistedAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(AppsToBeBlacklistedAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class AppsToBeBlacklistedAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView App { get; set; }
        public CheckBox blacklistToggle { get; set; }


        public AppsToBeBlacklistedAdapterViewHolder(View itemView, Action<AppsToBeBlacklistedAdapterClickEventArgs> clickListener,
                            Action<AppsToBeBlacklistedAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            App= itemView.FindViewById<TextView>(Resource.Id.applicationname);
            blacklistToggle = itemView.FindViewById<CheckBox>(Resource.Id.istheappblacklisted);

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
using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Android.Content.PM;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using Android.Util;

namespace LiveDisplay.Adapters
{
    class AppsToBeBlacklistedAdapter : RecyclerView.Adapter
    {
        List<PackageInfo> items;
        private int selectedItem = -1;

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
            return new AppsToBeBlacklistedAdapterViewHolder(itemView);
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {

            selectedItem = position;

            // Replace the contents of the view with that element
            var holder = viewHolder as AppsToBeBlacklistedAdapterViewHolder;
            holder.App.Text = PackageUtils.GetTheAppName(items[position].PackageName);
            holder.BlacklistToggle.Checked = Blacklist.IsAppBlacklisted(items[position].PackageName);
            holder.BlacklistToggle.CheckedChange += BlacklistToggle_CheckedChange;
            Log.Info("LiveDisplay", "CalledOnBindViewHolder");

        }

        private void BlacklistToggle_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                Blacklist.ToggleAppBlacklistState(items[selectedItem].PackageName, true);
            }
            else
            {
                Blacklist.ToggleAppBlacklistState(items[selectedItem].PackageName, false);

            }
        }

        public override int ItemCount => items.Count;

    }

    public class AppsToBeBlacklistedAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView App { get; set; }
        public CheckBox BlacklistToggle { get; set; }


        public AppsToBeBlacklistedAdapterViewHolder(View itemView) : base(itemView)
        {
            App= itemView.FindViewById<TextView>(Resource.Id.applicationname);
            BlacklistToggle = itemView.FindViewById<CheckBox>(Resource.Id.istheappblacklisted);   
        }

    }

}
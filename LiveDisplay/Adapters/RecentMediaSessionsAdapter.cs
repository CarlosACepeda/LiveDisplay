using Android.Views;
using AndroidX.RecyclerView.Widget;
using System.Collections.Generic;
using System;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Factories;
using AndroidX.AppCompat.Widget;
using LiveDisplay.Services;
using System.Linq;

namespace LiveDisplay.Adapters
{
    class RecentMediaSessionsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<RecentMediaSessionsAdapterClickEventArgs> ItemClick;
        public event EventHandler<RecentMediaSessionsAdapterClickEventArgs> ItemLongClick;
        ICollection<string> recentMediaSessions;

        public RecentMediaSessionsAdapter(ICollection<string> recentMediaSessions)
        {
            this.recentMediaSessions = recentMediaSessions;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            var id = Resource.Layout.recent_sessions_item;
            var itemView = LayoutInflater.From(parent.Context).
                   Inflate(id, parent, false);

            var vh = new RecentMediaSessionsAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = recentMediaSessions.ElementAt(position);

            // Replace the contents of the view with that element
            var holder = viewHolder as RecentMediaSessionsAdapterViewHolder;
            holder.AppTitle.Text = PackageUtils.GetTheAppName(item);
            holder.AppIcon.Background =
                new IconFactory(PackageUtils.GetAppIcon(item))
                                            //.ApplyColorFilter(Android.Graphics.Color.White)
                                            .ResizeDrawable(150, 150)
                                            .Build();

        }

        public override int ItemCount => recentMediaSessions.Count;

        void OnClick(RecentMediaSessionsAdapterClickEventArgs args) {
            KeyguardPendingIntentMediator.GetInstance().SendIntent(PackageUtils.GetAppIntent(recentMediaSessions.ElementAt(args.Position)));
        }
        void OnLongClick(RecentMediaSessionsAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class RecentMediaSessionsAdapterViewHolder : RecyclerView.ViewHolder
    {
        public AppCompatImageView AppIcon { get; set; }
        public TextView AppTitle { get; set; }


        public RecentMediaSessionsAdapterViewHolder(View itemView, Action<RecentMediaSessionsAdapterClickEventArgs> clickListener,
                            Action<RecentMediaSessionsAdapterClickEventArgs> longClickListener) : base(itemView)
        {

            AppIcon = itemView.FindViewById<AppCompatImageView>(Resource.Id.app_icon);
            AppTitle = itemView.FindViewById<TextView>(Resource.Id.app_name); 

            itemView.Click += (sender, e) => clickListener(new RecentMediaSessionsAdapterClickEventArgs { View = itemView, Position = AbsoluteAdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new RecentMediaSessionsAdapterClickEventArgs { View = itemView, Position = AbsoluteAdapterPosition });
        }
    }

    public class RecentMediaSessionsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}
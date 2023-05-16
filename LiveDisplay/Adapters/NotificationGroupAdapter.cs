using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using LiveDisplay.Models;
using LiveDisplay.Services.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDisplay.Adapters
{
    internal class NotificationGroupAdapter : RecyclerView.Adapter
    {

        List<OpenNotification> childrenNotifications;
        private int? notificationToShowId=null;
        public int notificationToShowPosition=-1; //Used by recyclerView to scroll to this position when needed.

        public NotificationGroupAdapter(List<OpenNotification> childrenNotifications, int? notificationToShowId =null)
        {
            this.childrenNotifications = childrenNotifications;
            this.notificationToShowId = notificationToShowId;
            if(notificationToShowId!= null)
                notificationToShowPosition = childrenNotifications.IndexOf(childrenNotifications.FirstOrDefault(p => p.Id == notificationToShowId));

        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var itemView = (NotificationGroupAdapterViewHolder)holder;
            OpenNotification openNotification = childrenNotifications[position];
            //TODO: apply styles here.
            itemView.app_name.Text = openNotification.ApplicationOwnerName;
            itemView.when.Text = openNotification.When;
            itemView.subtext.Text = openNotification.SubText;
            itemView.title.Text = openNotification.Title;
            itemView.text.Text = openNotification.Text;

        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(parent.Context);
            View itemView = layoutInflater.Inflate(Resource.Layout.notification_group_item, parent, false);
            return new NotificationGroupAdapterViewHolder(itemView, OnClick);
        }

        void OnClick(int itemPosition)
        {
            NotificationHijackerWorker.ClickNotification(childrenNotifications[itemPosition]);
        }

        public override int ItemCount => childrenNotifications.Count;
    }

    internal class NotificationGroupAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView app_name, when, subtext, title, text;
        public ImageButton toggle_collapse, close_notification, send_response; //Close notification should be by sliding, quicker, TODO: Remove
        public ProgressBar progress;
        public FrameLayout actions;
        public LinearLayout inline_response_container;
        public EditText inline_text;

        public NotificationGroupAdapterViewHolder(View itemView, Action<int> onClickListener) : base(itemView)
        {
            app_name = itemView.FindViewById<TextView>(Resource.Id.app_name);
            when = itemView.FindViewById<TextView>(Resource.Id.when);
            subtext = itemView.FindViewById<TextView>(Resource.Id.subtext);
            title = itemView.FindViewById<TextView>(Resource.Id.title);
            text = itemView.FindViewById<TextView>(Resource.Id.text);
            itemView.Click += (sender, e) => onClickListener(AdapterPosition);
        }
    }
}
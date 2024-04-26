namespace LiveDisplay.Adapters
{
    using Android.App;
    using Android.OS;
    using Android.Util;
    using Android.Views;
    using Android.Widget;
    using AndroidX.RecyclerView.Widget;
    using LiveDisplay.Factories;
    using LiveDisplay.Misc;
    using LiveDisplay.Models;
    using LiveDisplay.Services;
    using LiveDisplay.Services.Music;
    using LiveDisplay.Services.Notifications;
    using LiveDisplay.Services.Notifications.NotificationEventArgs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Android.App.DownloadManager;

    public class NotificationAdapter : RecyclerView.Adapter
    {
        public static int selectedItem = -1;
        public List<OpenNotification> notifications = new List<OpenNotification>();
        public override int ItemCount => notifications.Count;

        public NotificationAdapter(List<OpenNotification> notificaciones)
        {
            get
            {
                return Build.VERSION.SdkInt < BuildVersionCodes.N ? 
                    singleNotifications.Count : groupedNotifications.Count;
            }
        }

        public NotificationAdapter(List<OpenNotification> notifications)
        {
            notifications = SortNotifications(notifications);
            GroupNotifications(notifications);
        }
        public void GroupNotifications(List<OpenNotification> notifications)
        {
                foreach (var openNotification in notifications)
                {
                    InsertIntoList(openNotification);
                }
        }

        private List<OpenNotification> SortNotifications(List<OpenNotification> notifications)
        {
            //Ordering notifications:
            //1st: Standalone notifs.
            //2st: Children notifs.
            //3st: Summary notifs.
            //4th: nothing, just to fill the operator.
            //That's because when handling summary notifications for the first time, we need the children already in place,
            //Or subsequent manipulations done to the summary notification requiring their children won't work at all.
            return notifications.OrderBy(x =>
                x.IsStandalone == true ? 1 :
                x.BelongsToGroup && !x.IsSummary == true ? 2 :
                x.IsSummary? 3:
                4
                ).ToList();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (position != RecyclerView.NoPosition)
            {
                //Cast
                NotificationAdapterViewHolder viewHolder = holder as NotificationAdapterViewHolder;

                viewHolder.Icono.Background = notifications[position].GetSmallIcon().LoadDrawable(Application.Context);
                if (selectedItem == position)
                {
                    groupedNotifications.RemoveAt(notificationPosition);
                    groupedNotifications.Add(notification);
                    NotifyItemChanged(notificationPosition);
                }
                else
                {
                    groupedNotifications.Add(notification);
                    NotifyItemInserted(groupedNotifications.Count - 1);
                }
            }

        }

        public void RemoveNotification(OpenNotification openNotification)
        {

            if (openNotification.IsSummary)
                RemoveSummaryNotification(openNotification);

            if (openNotification.IsStandalone)
                RemoveStandaloneNotification(openNotification);

            if (openNotification.BelongsToGroup)
                RemoveChildNotification(openNotification);


            OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
            {
                ThereAreNotifications = NotificationHijackerWorker.DeviceSupportsNotificationGrouping() ?
                groupedNotifications.Where(n => n.IsRemovable).ToList().Count > 0 :
                singleNotifications.Where(n => n.IsRemovable).ToList().Count > 0
            });


        }

        private void RemoveChildNotification(OpenNotification openNotification)
        {
            int notificationPosition;
            notificationPosition = GetItemPosition(openNotification, false);
            if (notificationPosition != -1)
            {
                singleNotifications.RemoveAt(notificationPosition);
                int parentNotificationPosition = GetParentNotificationPosition(openNotification);
                NotifyItemChanged(parentNotificationPosition);
            }
            OnNotificationRemoved(openNotification);
        }

        private void RemoveStandaloneNotification(OpenNotification openNotification)
        {
            int notificationPosition;

            if (Build.VERSION.SdkInt < BuildVersionCodes.N)
            {
                notificationPosition = GetItemPosition(openNotification, false);
                if (notificationPosition != -1)
                {
                    singleNotifications.RemoveAt(notificationPosition);
                    NotifyItemRemoved(notificationPosition);
                }

            }
            else
            {
                notificationPosition = GetItemPosition(openNotification, true);
                if (notificationPosition != -1)
                {
                    groupedNotifications.RemoveAt(notificationPosition);
                    NotifyItemRemoved(notificationPosition);
                }
            }
            OnNotificationRemoved(openNotification);

        }

        private void RemoveSummaryNotification(OpenNotification openNotification)
        {
            int notificationPosition;
            notificationPosition = GetItemPosition(openNotification, true);
            if (notificationPosition != -1)
            {
                groupedNotifications.RemoveAt(notificationPosition);
                NotifyItemRemoved(notificationPosition);
            }
            OnNotificationRemoved(openNotification);
        }
        int GetParentNotificationPosition(OpenNotification child)
        {
            OpenNotification parent = groupedNotifications.FirstOrDefault(p => p.GroupKey == child.GroupKey && p.IsSummary);
            if (parent == null) return -1;

            return groupedNotifications.IndexOf(parent);
        }

        int GetChildNotificationCount(OpenNotification openNotification)
        {
            if (openNotification.IsSummary)
                return singleNotifications.Count(child => child.BelongsToGroup && child.GroupKey == openNotification.GroupKey);
            else return 0;
        }
        int GetSiblingNotificationCount(OpenNotification openNotification)
        {
             return singleNotifications.Count(child => child.BelongsToGroup && child.GroupKey == openNotification.GroupKey);
        }
        public bool NotificationHasSiblings(OpenNotification openNotification)
        {
            if (!NotificationHijackerWorker.DeviceSupportsNotificationGrouping()) return false;
            return singleNotifications.Count(on => on.GroupKey == openNotification?.GroupKey)>1;
        }
        List<OpenNotification> GetSiblings(OpenNotification aSibling)
        {
            if (!NotificationHijackerWorker.DeviceSupportsNotificationGrouping()) return null;
            return singleNotifications.Where(on => on.GroupKey == aSibling?.GroupKey).ToList();
        }

        List<OpenNotification> GetOrphanNotifications(OpenNotification newParent)
        {
            //Orphan notifications live in the 'grouped' list because before this method call, they are considered to be their own 
            //parent, so they live here.
            return groupedNotifications.Where(child => child.GroupKey == newParent.GroupKey && child.BelongsToGroup).ToList();
        }

        private int GetItemPosition(OpenNotification openNotification, bool searchInGroupedList)
        {
            if(searchInGroupedList)
                return groupedNotifications.IndexOf(groupedNotifications.FirstOrDefault
                (o => o.Id == openNotification.Id && o.ApplicationPackage == openNotification.ApplicationPackage && o.Tag == openNotification.Tag &&
            o.IsSummary == openNotification.IsSummary));
            else
                return singleNotifications.IndexOf(singleNotifications.FirstOrDefault
                (o => o.Id == openNotification.Id && o.ApplicationPackage == openNotification.ApplicationPackage && o.Tag == openNotification.Tag));
        }

        OpenNotification GetChildNotification(OpenNotification parent)
        {
            return singleNotifications.FirstOrDefault(child => child.GroupKey == parent.GroupKey && child.BelongsToGroup); //we grab the first child found
        }
        List<OpenNotification> GetChildrenNotifications(OpenNotification parent)
        {
            return singleNotifications.Where(child => child.GroupKey == parent.GroupKey && child.BelongsToGroup).ToList();
        }
        private bool IsSystemSummary(OpenNotification summaryNotification)
        {
            return summaryNotification.GroupKey.Contains("g:ranker_group");
        }
        private void OnNotificationRemoved(OpenNotification sbn)
        {
            NotificationRemoved?.Invoke(null, new NotificationRemovedEventArgs
            {
                OpenNotification= sbn
            });
        }
        private void OnNotificationPosted(OpenNotification sbn)
        {
            var children = GetChildrenNotifications(sbn);
            var siblings = GetSiblings(sbn);
            bool isParent = children != null && children.Count > 0;
            bool isSibling = !isParent && NotificationHasSiblings(sbn);
            bool isStandalone = !isParent && (children == null || children.Count == 0);

            NotificationPosted?.Invoke(null, new NotificationPostedEventArgs
            {
                NotificationPosted= sbn,
                ShouldCauseWakeUp= true, //TODO: This should be set by 'HandlexxxxNotification()' Methods
                UpdatesPreviousNotification= true, //TODO: This should be set by 'HandlexxxxNotification()' Methods
                Children= GetChildrenNotifications(sbn),
                Siblings= GetSiblings(sbn),
                IsSibling= isSibling,
                IsParent= isParent,
                IsStandalone= isStandalone
        });
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(parent.Context);
            View itemView = layoutInflater.Inflate(Resource.Layout.NotificationItemRow, parent, false);
            return new NotificationAdapterViewHolder(itemView, OnClick, OnLongClick);
        }

        private void ItemView_LongClick(object sender, View.LongClickEventArgs e)
        {
            var statusBarNotification = CatcherHelper.OpenNotifications[LayoutPosition];
            OnItemLongClicked(LayoutPosition, statusBarNotification);
        }

        private void ItemView_Click(object sender, EventArgs e)
        {
            //Simply indicates which item was clicked and after that call NotifyDataSetChanged to changes take effect.
            NotificationAdapter.selectedItem = LayoutPosition;
            //CatcherHelper.notificationAdapter.NotifyDataSetChanged();
            var statusBarNotification = CatcherHelper.OpenNotifications[LayoutPosition];
            OnItemClicked(LayoutPosition, statusBarNotification);
            //try
            //{
            //    var view = sender as View;
            //    view.Visibility = ViewStates.Gone;
            //}
            //catch (Exception ex)
            //{
            //    Log.Info("LiveDisplay", "Exception hiding notification" + ex.Message);
            //}
        }

        private void OnItemClicked(int position, OpenNotification sbn)
        {
            ItemClick?.Invoke(null, new NotificationItemClickedEventArgs
            {
                Position = position,
                OpenNotification = sbn
            });
        }

        private void OnItemLongClicked(int position, OpenNotification sbn)
        {
            ItemLongClick?.Invoke(null, new NotificationItemClickedEventArgs
            {
                Position = position,
                OpenNotification = sbn
            }
            );
        }

        private void OnNotificationListSizeChanged(NotificationListSizeChangedEventArgs e)
        {
            NotificationListSizeChanged?.Invoke(null, e);
        }

    }

    //The following class just simply saves the view's references to the row, in order to avoid making calls to 'FindViewById' each time, nothing more is done here.
    internal class NotificationAdapterViewHolder : RecyclerView.ViewHolder
    {
        public ImageView Icono { get; set; }
        public TextView NotificationCount { get; set; }
        public OpenNotification OpenNotification { get; set; }

        public NotificationAdapterViewHolder(View itemView, Action<NotificationAdapterClickEventArgs> clickListener,
                            Action<NotificationAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            Icono = itemView.FindViewById<ImageView>(Resource.Id.icon);
            NotificationCount = itemView.FindViewById<TextView>(Resource.Id.notification_count);

            itemView.Click += (sender, e) => clickListener(new NotificationAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new NotificationAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
        
    }
    public class NotificationAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}
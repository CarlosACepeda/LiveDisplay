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
        List<OpenNotification> singleNotifications = new List<OpenNotification>();
        List<OpenNotification> groupedNotifications = new List<OpenNotification>();
        public static event EventHandler<NotificationListSizeChangedEventArgs> NotificationListSizeChanged;
        Handler uiThreadHandler;


        public static event EventHandler<NotificationItemClickedEventArgs> ItemClick;
        public static event EventHandler<NotificationItemClickedEventArgs> ItemLongClick;

        public static event EventHandler<NotificationRemovedEventArgs> NotificationRemoved;

        public static event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        public override int ItemCount
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
                OpenNotification item;

                if (Build.VERSION.SdkInt < BuildVersionCodes.N)
                    item = singleNotifications[position];
                else
                    item= groupedNotifications[position];

                if (MusicController.MediaSessionAssociatedWThisNotification(item.GetCustomId)
                    && new ConfigurationManager(AppPreferences.Default).RetrieveAValue(ConfigurationParameters.HideNotificationWhenItsMediaPlaying)
                    && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    holder.ItemView.Visibility = ViewStates.Gone;
                    holder.ItemView.LayoutParameters = new RecyclerView.LayoutParams(0, 0);
                }
                else
                {
                    //Cast
                    NotificationAdapterViewHolder viewHolder = holder as NotificationAdapterViewHolder;
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                    {
                        viewHolder.Icono.Background = IconFactory.ReturnIconDrawable(item.SmallIcon, item.ApplicationPackage);
                    }
                    else
                    {
                        viewHolder.Icono.Background = IconFactory.ReturnIconDrawable(item.IconResourceInt, item.ApplicationPackage);
                    }
                    if (GetChildNotificationCount(item)>1)
                    {
                        viewHolder.NotificationCount.Text = GetChildNotificationCount(item).ToString();
                        viewHolder.NotificationCount.Visibility = ViewStates.Visible;
                    }
                    else
                        viewHolder.NotificationCount.Visibility = ViewStates.Gone;

                }
            }
            else
            {
                Log.Info("LiveDisplay", "WTF Position: " + position);
            }
            
        }

        public void InsertIntoList(OpenNotification openNotification)
        {
            if (openNotification.IsSummary)
                HandleSummaryNotification(openNotification);

            if (openNotification.IsStandalone)
                HandleStandaloneNotification(openNotification);

            if (openNotification.BelongsToGroup && !openNotification.IsSummary) //Summary notifications also belong to the group. so we exclude it here.
                HandleChildNotification(openNotification);

            OnNotificationPosted(openNotification);

            OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
            {
                ThereAreNotifications = NotificationHijackerWorker.DeviceSupportsNotificationGrouping() ?
                groupedNotifications.Where(n => n.IsRemovable).ToList().Count > 0 :
                singleNotifications.Where(n => n.IsRemovable).ToList().Count > 0
            });

        }

        void HandleSummaryNotification(OpenNotification notification)
        {
            int notificationPosition = GetItemPosition(notification, true);
           
            if (notificationPosition!= -1)
            {
                groupedNotifications.RemoveAt(notificationPosition);
                groupedNotifications.Add(notification);
                NotifyItemChanged(notificationPosition);
            }
            else
            {
                if (IsSystemSummary(notification))
                {
                    //system summary notifications don't carry any information, so we must set it ourselves,
                    //else the lockscreen shows this notification without text.
                    notification= FillSystemSummaryNotification(notification);
                }
                //Checking if Automatic Grouping happened
                //Android groups automatically a set of notifications is the app issuing them doesn't do so.
                //(Provides a parent for a set of individual notifications that are meant to be grouped)
                //So we must do the same
                if (NotificationHasSiblings(notification)) //If it returns true it means this new notification is meant to be the parent
                    //of the subsequent children.
                {
                    var orphanNotifications = GetOrphanNotifications(notification);
                    foreach (OpenNotification orphan in orphanNotifications)
                    {
                        RemoveStandaloneNotification(orphan); //Up until now, they were their own parent 
                        HandleChildNotification(orphan); //Now we add them as a child.
                    }
                    groupedNotifications.Add(notification); //Finally we add the parent to the list.

                }
                else
                {
                    groupedNotifications.Add(notification);
                }
                NotifyItemInserted(groupedNotifications.Count - 1);
            }
        }

        private OpenNotification FillSystemSummaryNotification(OpenNotification notification)
        {
            List<OpenNotification> children = GetChildrenNotifications(notification);
            string title;
            string text = null;
            foreach(var child in children)
            {
                text = text + string.Concat(child.Title, "  ", child.Text, "\n");
            }
            title = notification.ApplicationOwnerName;
            notification.Title = title;
            notification.Text = text;

            return notification;
        }

        void HandleChildNotification(OpenNotification notification) //It is a notification that's part of a group.
        {
            int notificationPosition = GetItemPosition(notification, false);

            if (notificationPosition != -1)
            {
                singleNotifications.RemoveAt(notificationPosition);
                singleNotifications.Add(notification);
            }
            else
            {
                singleNotifications.Add(notification);
            }

        }
        void HandleStandaloneNotification(OpenNotification notification)
        {
            int notificationPosition;

            if (Build.VERSION.SdkInt < BuildVersionCodes.N)
            {
                notificationPosition = GetItemPosition(notification, false);
                if (notificationPosition != -1)
                {
                    singleNotifications.RemoveAt(notificationPosition);
                    singleNotifications.Add(notification);
                    uiThreadHandler?.Post(()=>
                    NotifyItemChanged(notificationPosition)) ;
                }
                else
                {
                    singleNotifications.Add(notification);
                    uiThreadHandler?.Post(() =>
                    NotifyItemInserted(singleNotifications.Count - 1));
                }
            }
            else
            {
                notificationPosition = GetItemPosition(notification, true);
                if (notificationPosition != -1)
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

        void OnClick(NotificationAdapterClickEventArgs args)
        {
            if (args.Position != RecyclerView.NoPosition)
            {
                OpenNotification notificationToSend = GetClickedNotification(args);
                var children = GetChildrenNotifications(notificationToSend);
                bool isParent = children != null && children.Count > 0;
                bool isStandalone= !isParent && (children == null || children.Count == 0);
                bool isSibling = !isParent && NotificationHasSiblings(notificationToSend);
                int siblingCount = GetSiblingNotificationCount(notificationToSend);
                int childCount = GetChildNotificationCount(notificationToSend);
                var siblings = GetSiblings(notificationToSend);
                OnItemClicked(args.Position, notificationToSend, children, isParent, isStandalone, isSibling, siblingCount, childCount, siblings);
            }
        }

        private OpenNotification GetClickedNotification(NotificationAdapterClickEventArgs args)
        {
            OpenNotification notificationToSend;
            if (NotificationHijackerWorker.DeviceSupportsNotificationGrouping())
            {
                notificationToSend = groupedNotifications[args.Position];
                //If it has only one child it means we must send the child as the parent,
                //because in this case, the parent doesn't contain the information to show on the Notification Widget,
                //like Title, subtext, etc, the child does.
                if (GetChildNotificationCount(notificationToSend) == 1)
                {
                    notificationToSend = GetChildNotification(notificationToSend);
                }
            }
            else
            {
                notificationToSend = singleNotifications[args.Position];
            }

            return notificationToSend;
        }

        void OnLongClick(NotificationAdapterClickEventArgs args)
        {
            OpenNotification notificationToSend = GetClickedNotification(args);
            OnItemLongClicked(args.Position, notificationToSend, args.View.GetX(), args.View.Width);
        }
        private void OnItemClicked(int position, OpenNotification sbn,
            List<OpenNotification> children, bool isParent, bool isStandalone, bool isSibling, int siblingCount, int childCount, List<OpenNotification> siblings)
        {
            ItemClick?.Invoke(null, new NotificationItemClickedEventArgs
            {
                Position = position,
                StatusBarNotification = sbn,
                Children = children,
                IsParent = isParent,
                IsStandalone = isStandalone,
                IsSibling = isSibling,
                SiblingCount = siblingCount,
                ChildCount = childCount,
                Siblings = siblings,
                IsFakeParent= sbn.GroupKey.Contains("g:ranker_group")
            }) ;
        }

        private void OnItemLongClicked(int position, OpenNotification sbn, float notificationViewX, int notificationViewWidth)
        {
            ItemLongClick?.Invoke(null, new NotificationItemClickedEventArgs
            {
                Position = position,
                StatusBarNotification = sbn,
                NotificationViewX= notificationViewX,
                NotificationViewWidth= notificationViewWidth
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
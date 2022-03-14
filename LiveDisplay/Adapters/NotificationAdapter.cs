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

    public class NotificationAdapter : RecyclerView.Adapter
    {
        List<OpenNotification> singleNotifications = new List<OpenNotification>();
        List<OpenNotification> groupedNotifications = new List<OpenNotification>();
        public static event EventHandler<NotificationListSizeChangedEventArgs> NotificationListSizeChanged;


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
            GroupNotifications(notifications);
        }
        public void GroupNotifications(List<OpenNotification> notifications)
        {
            if (NotificationHijackerWorker.DeviceSupportsNotificationGrouping())
            {
                foreach (var openNotification in notifications)
                {
                    if (openNotification.IsSummary ||
                        (!openNotification.IsSummary && !openNotification.BelongsToGroup))
                    {
                        groupedNotifications.Add(openNotification);
                    }
                    else
                    {
                        singleNotifications.Add(openNotification); //Is a standalone notification
                    }
                }

            }
            else
            {
                singleNotifications = notifications; // no grouping made.
            }
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

            if (openNotification.BelongsToGroup)
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
                groupedNotifications.Add(notification);
                NotifyItemInserted(groupedNotifications.Count - 1);
            }
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
                    NotifyItemChanged(notificationPosition);
                }
                else
                {
                    singleNotifications.Add(notification);
                    NotifyItemInserted(singleNotifications.Count - 1);
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

        private int GetItemPosition(OpenNotification openNotification, bool searchInGroupedList)
        {
            if(searchInGroupedList)
                return groupedNotifications.IndexOf(groupedNotifications.FirstOrDefault
                (o => o.Id == openNotification.Id && o.ApplicationPackage == openNotification.ApplicationPackage && o.Tag == openNotification.Tag &&
            o.IsSummary == openNotification.IsSummary));
            else
                return singleNotifications.IndexOf(singleNotifications.FirstOrDefault
                (o => o.Id == openNotification.Id && o.ApplicationPackage == openNotification.ApplicationPackage && o.Tag == openNotification.Tag &&
            o.IsSummary == openNotification.IsSummary));
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
            NotificationPosted?.Invoke(null, new NotificationPostedEventArgs
            {
                NotificationPosted= sbn,
                ShouldCauseWakeUp= true,
                UpdatesPreviousNotification= true
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
                var statusBarNotification = groupedNotifications[args.Position];
                OnItemClicked(args.Position, statusBarNotification);
            }
        }

        void OnLongClick(NotificationAdapterClickEventArgs args)
        {
            var statusBarNotification = groupedNotifications[args.Position];
            OnItemLongClicked(args.Position, statusBarNotification);
        }
        private void OnItemClicked(int position, OpenNotification sbn)
        {
            ItemClick?.Invoke(null, new NotificationItemClickedEventArgs
            {
                Position = position,
                StatusBarNotification = sbn,
            });
        }

        private void OnItemLongClicked(int position, OpenNotification sbn)
        {
            ItemLongClick?.Invoke(null, new NotificationItemClickedEventArgs
            {
                Position = position,
                StatusBarNotification = sbn,
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
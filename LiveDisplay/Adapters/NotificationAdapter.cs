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
            GroupNotifications(notifications);
        }
        public void GroupNotifications(List<OpenNotification> notifications)
        {
            if (NotificationHijackerWorker.DeviceSupportsNotificationGrouping())
            {
                foreach (var openNotification in notifications)
                {
                    if (openNotification.IsSummary ||
                        openNotification.IsStandalone)
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
            if(uiThreadHandler== null) { uiThreadHandler = new Handler(holder.ItemView.Context.MainLooper); }
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
        public bool NotificationHasSiblings(OpenNotification openNotification)
        {
            if (!NotificationHijackerWorker.DeviceSupportsNotificationGrouping()) return false;
            return singleNotifications.Count(on => on.GroupKey == openNotification?.GroupKey)>1;
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
                OpenNotification notificationToSend = GetClickedNotification(args);
                OnItemClicked(args.Position, notificationToSend);  
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
        private void OnItemClicked(int position, OpenNotification sbn)
        {
            ItemClick?.Invoke(null, new NotificationItemClickedEventArgs
            {
                Position = position,
                StatusBarNotification = sbn,
            });
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
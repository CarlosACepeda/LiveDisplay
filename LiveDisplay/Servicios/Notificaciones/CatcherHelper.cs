using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Service.Notification;
using Android.Views;
using Android.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Servicios.Notificaciones.NotificationEventArgs;

namespace LiveDisplay.Servicios.Notificaciones
{
    /// <summary>
    /// This class is made for Help the main Catcher to do its Actions like:
    /// Update a notification.
    /// Insert a Notification.
    /// Initialize the data (?) maybe.
    /// Group a notification, this is a new feature in Nougat(API Level 24+)
    /// Also is made to allow more funcionality and keep the Catcher Service readable and understandable 
    /// (for me and others, gracias al cielo) 
    /// </summary>
    class CatcherHelper:Java.Lang.Object
    {
        public static NotificationAdapter notificationAdapter;
        public static List<StatusBarNotification> statusBarNotifications;
        //Events that this class will trigger to Notify LockScreen about these
        //When RecyclerView will be removed
#pragma warning disable CS0067 // El evento 'CatcherHelper.NotificationPosted' nunca se usa
        public static event EventHandler NotificationPosted; //NotifyItemInserted.
#pragma warning restore CS0067 // El evento 'CatcherHelper.NotificationPosted' nunca se usa
#pragma warning disable CS0067 // El evento 'CatcherHelper.NotificationUpdated' nunca se usa
        public static event EventHandler NotificationUpdated; //NotifyItemUpdated.
#pragma warning restore CS0067 // El evento 'CatcherHelper.NotificationUpdated' nunca se usa
#pragma warning disable CS0067 // El evento 'CatcherHelper.NotificationGrouped' nunca se usa
        public static event EventHandler NotificationGrouped; // TODO: Clueless (?) :'-( I don't know how to implement this in LockScreen
#pragma warning restore CS0067 // El evento 'CatcherHelper.NotificationGrouped' nunca se usa
#pragma warning disable CS0067 // El evento 'CatcherHelper.NotificationUngrouped' nunca se usa
        public static event EventHandler NotificationUngrouped; //TODO
#pragma warning restore CS0067 // El evento 'CatcherHelper.NotificationUngrouped' nunca se usa
        public static event EventHandler<NotificationListSizeChangedEventArgs> NotificationListSizeChanged;
        public static bool thereAreNotifications = false;
        /// <summary>
        /// Constructor of the Class
        /// </summary>
        /// <param name="statusBarNotifications">This list is sent by Catcher, and is used to fill the Adapter
        /// that the RecyclerView will use.
        /// </param>
        public CatcherHelper(List<StatusBarNotification> statusBarNotifications)
        {
            CatcherHelper.statusBarNotifications = statusBarNotifications;
            notificationAdapter = new NotificationAdapter(statusBarNotifications);
            if (statusBarNotifications.Count > 0)
            {
                thereAreNotifications = true;
            }

        }
        //If Catcher call this, it means that the notification is part of a Group of notifications and should be Grouped.
        private void GroupNotification()
        {
            //After this, fire event.
            //Find ID, if Found, Append to that notification, if not WtF. lol.
        }
        public void InsertNotification(StatusBarNotification sbn)
        {
            int indice = GetNotificationPosition(sbn);
            
            if (indice>=0)
            {
                statusBarNotifications.RemoveAt(indice);
                statusBarNotifications.Add(sbn);
                notificationAdapter.NotifyItemChanged(indice);
                Console.WriteLine("Notification Updated");                          
            }
            else
            {              
                try
                {
                    statusBarNotifications.Add(sbn);
                    notificationAdapter.NotifyItemInserted(indice);

                    Console.WriteLine("Notification Inserted");
                    if (ScreenOnOffReceiver.isScreenOn == false)
                    {
                        //Awake.WakeUpScreen can be configurable by blacklist;
                        Awake.WakeUpScreenOnNewNotification(sbn.PackageName);                     
                        Awake.LockScreen();
                        //Start a user configurable timer to Sleep again device;
                        Console.WriteLine("Awake device");
                    }
                    

                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error is" + ex);
                }
                
            }
            if (statusBarNotifications.Count > 0)
            {
                OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
                {
                    ThereAreNotifications = true
                });
                thereAreNotifications = true;
            }

        }
        /// <summary>
        ///If Catcher call this, it means that the notification exist, is not part of a Group and should be UPDATED.
        ///not Appended as seen in GroupNotification()
        /// </summary>
        /// <param name="whichNotification">Id of the Notification to be Updated.</param>
        private void UpdateNotification(StatusBarNotification sbn)
        {
            int position = GetNotificationPosition(sbn);
            //After this, fire event.
            //Call GroupNotification if needed.
            if (position>=0)
            {
                statusBarNotifications.RemoveAt(position);
                statusBarNotifications.Add(sbn);
            }
            
            notificationAdapter.NotifyItemChanged(position);
            Console.WriteLine("NotificationUpdated");
        }
        private void RemoveNotificationFromGroup()
        {
            //After this, fire event.
        }
        public void RemoveNotification(StatusBarNotification sbn)
        {
            int position = GetNotificationPosition(sbn);
            if (position >= 0)
            {
                statusBarNotifications.RemoveAt(position);

                notificationAdapter.NotifyItemRemoved(position);
            }
            //Check if when removing this notification the list size is zero, if true, then raise an event that will
            //indicate the lockscreen to hide the 'Clear all button'
            if (statusBarNotifications.Count == 0)
            {
                OnNotificationListSizeChanged(new NotificationListSizeChangedEventArgs
                { ThereAreNotifications=false
                });
                thereAreNotifications = false;
            }
        }
        public void CancelAllNotifications()
        {
            notificationAdapter.NotifyDataSetChanged();
        }

        private int GetNotificationPosition(StatusBarNotification sbn)
        {
            int index = statusBarNotifications.IndexOf(statusBarNotifications.FirstOrDefault(o => o.Id == sbn.Id && o.PackageName == sbn.PackageName));

            return index;
        }

        //Make an method that will fire an event that Lockscreen will be subscribed to
        //This method will Invoke the event when there are not notifications on the list
        //also when the List of notifications has items, 
        //So Lockscreen can react to this event and Hide/Show the 'Clear all' button depending
        //on if either there are or there aren't notifications on the list

        private void OnNotificationListSizeChanged(NotificationListSizeChangedEventArgs e)
        {
            //TODO: Implement me 
            NotificationListSizeChanged?.Invoke(this, e);
        }

    }
}
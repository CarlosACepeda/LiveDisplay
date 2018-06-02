using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Service.Notification;
using Android.Views;
using Android.Widget;
using LiveDisplay.Adapters;
using LiveDisplay.BroadcastReceivers;

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
    class CatcherHelper
    {
        public static NotificationAdapter notificationAdapter;
        public static List<StatusBarNotification> statusBarNotifications;
        //Events that this class will trigger to Notify LockScreen about these
        //When RecyclerView will be removed
        public event EventHandler OnNotificationPosted; //NotifyItemInserted.
        public event EventHandler OnNotificationUpdated; //NotifyItemUpdated.
        public event EventHandler OnNotificationGrouped; // TODO: Clueless (?) :'-( I don't know how to implement this in LockScreen
        public event EventHandler OnNotificationUngrouped; //TODO

        /// <summary>
        /// Contructor of the Class
        /// </summary>
        /// <param name="statusBarNotifications">This list is sent by Catcher, and is used to fill the Adapter
        /// that the RecyclerView will use.
        /// </param>
        public CatcherHelper(List<StatusBarNotification> statusBarNotifications)
        {
            CatcherHelper.statusBarNotifications = statusBarNotifications;
            notificationAdapter = new NotificationAdapter(statusBarNotifications);
        }
        //If Catcher call this, it means that the notification is part of a Group of notifications and should be Grouped.
        private void GroupNotification()
        {
            //After this, fire event.
            //Find ID, if Found, Append to that notification, if not WtF. lol.
        }
        public void InsertNotification(StatusBarNotification sbn)
        {
            bool foundNotification = FindNotification(sbn.Id, sbn.PackageName);
            //After this fire event.
            //Check if the Notification is part of a group and if a Notification with that Id exists to Append that Group.
            //if (Build.VERSION.SdkInt > BuildVersionCodes.N)
            //{                
            //    if(sbn.IsGroup==true&& FindNotification(sbn.Id, sbn.PackageName)==true)
            //    {
            //        //TODO
            //        //Should I define a counter to know how many Notifications are part of this Group?
            //        GroupNotification();
            //    }
            //}
            //For Marshmallow and below: Check if Notification Exists, if true, update it, if not, Insert one.
            if (foundNotification == true)
            {
                UpdateNotification(sbn);
            }
            else
            {
                statusBarNotifications.Add(sbn);
                int position = GetNotificationPosition(sbn.Id, sbn.PackageName);
                try
                {
                    notificationAdapter.NotifyItemInserted(position);
                    if (ScreenOnOffReceiver.isScreenOn == false)
                    {
                    Intent lockScreenIntent = new Intent(Application.Context, typeof(LockScreenActivity));
                        Bundle b = new Bundle();
                        b.PutInt("wake", 1);
                    Application.Context.StartActivity(lockScreenIntent);
                    }
                    
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error is" + ex);
                }
                
            }

        }
        /// <summary>
        ///If Catcher call this, it means that the notification exist, is not part of a Group and should be UPDATED.
        ///not Appended as seen in GroupNotification()
        /// </summary>
        /// <param name="whichNotification">Id of the Notification to be Updated.</param>
        private void UpdateNotification(StatusBarNotification sbn)
        {
            int position = GetNotificationPosition(sbn.Id, sbn.PackageName);
            //After this, fire event.
            //Call GroupNotification if needed.
            statusBarNotifications.RemoveAt(position);
            statusBarNotifications.Add(sbn);
            notificationAdapter.NotifyItemChanged(position);
        }
        private void RemoveNotificationFromGroup()
        {
            //After this, fire event.
        }
        public void RemoveNotification(StatusBarNotification sbn)
        {
            int position = GetNotificationPosition(sbn.Id, sbn.PackageName);
            statusBarNotifications.RemoveAt(position);
            notificationAdapter.NotifyItemRemoved(position);
        }
        public void CancelAllNotifications()
        {
            notificationAdapter.NotifyDataSetChanged();
        }

        /// <summary>
        /// Method that finds a Notification within the list of Notifications using an Id and the Package of the app
        /// that the notification belongs to.
        /// </summary>
        /// <param name="which">the id of the Notification to find</param>
        /// <param name="package">the package(app) that this notification belongs to</param>
        /// <returns>Returns true if found a Notification, false otherwise</returns>
        public bool FindNotification(int which, string package)
        {
            int? index = statusBarNotifications.IndexOf(statusBarNotifications.FirstOrDefault(o => o.Id == which && o.PackageName == package));
            if (index >0)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
        private int GetNotificationPosition(int which, string package)
        {
           int index = statusBarNotifications.IndexOf(statusBarNotifications.FirstOrDefault(o => o.Id == which && o.PackageName == package));
            
           return Convert.ToInt32(index);
        }
    }
}
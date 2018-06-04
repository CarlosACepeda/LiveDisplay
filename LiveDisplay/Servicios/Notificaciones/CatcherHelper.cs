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
            Console.WriteLine("Bound adapter");
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
                     PowerManager pm = ((PowerManager)Application.Context.GetSystemService(Context.PowerService));
                    var screenLock = pm.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, "Turn On Lockscreen");
                    screenLock.Acquire();
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            Thread.Sleep(5000);
                            screenLock.Release();
                            LockScreen();
                        });
                        //Start a user configurable timer to Sleep again device;
                        Console.WriteLine("Awake device");
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
            
            
            Console.WriteLine("NotificationRemoved");
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
        /// <summary>
        /// Lock/Turn off the screen TODO: Move to Awake class (returns)
        /// </summary>
        private void LockScreen()
        {
            PowerManager pm = ((PowerManager)Application.Context.GetSystemService(Context.PowerService));
            DevicePolicyManager policy;
            if (Build.VERSION.SdkInt < BuildVersionCodes.KitkatWatch)
            {
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
                if (pm.IsScreenOn == true)
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
                {
                    policy = (DevicePolicyManager)Application.Context.GetSystemService(Context.DevicePolicyService);
                    try
                    {
                        policy.LockNow();
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        Console.WriteLine(ex); 
                        Toast.MakeText(Application.Context, "Must enable dev admin", ToastLength.Long).Show();
                        ComponentName admin = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));
                        Intent intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin).PutExtra(DevicePolicyManager.ExtraDeviceAdmin, admin);
                        Application.Context.StartActivity(intent);
                    }
                }
            }
            else
            {
                if (pm.IsInteractive == true)
                {
                    policy = (DevicePolicyManager)Application.Context.GetSystemService(Context.DevicePolicyService);
                    try
                    {
                        policy.LockNow();
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        Console.WriteLine(ex);
                        Toast.MakeText(Application.Context, "Must enable dev admin", ToastLength.Long).Show();
                        ComponentName admin = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof( AdminReceiver)));
                        Intent intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin).PutExtra(DevicePolicyManager.ExtraDeviceAdmin, admin);
                        Application.Context.StartActivity(intent);
                    }
                }
            }
        }
    }
}
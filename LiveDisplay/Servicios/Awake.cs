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
using Android.Views;
using Android.Widget;
using LiveDisplay.BroadcastReceivers;

namespace LiveDisplay.Servicios
{
    /// <summary>
    /// This class will handle everything related to: Wake up the screen on User movement, 
    /// wake up the screen when user picks the phone out of his pocket.
    /// wake up the screen on New notification posted when screen is turned off.
    /// Turn off the screen programatically providing it with the custom time in which the screen should
    /// remain turned on
    /// 
    /// </summary>
    class Awake
    {
        /// <summary>
        /// Method that will wake up the screen when user picks up the phone from a plain surface
        /// ,user picks up the phone out of his pocket and when a new Notification is posted.
        /// called by CatcherService
        /// </summary>
        public static void WakeUpScreen()
        {
            PowerManager pm = ((PowerManager)Application.Context.GetSystemService(Context.PowerService));
            var screenLock = pm.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, "Turn On Lockscreen");
            screenLock.Acquire();
            ThreadPool.QueueUserWorkItem(o =>
            {
                Thread.Sleep(500);
                if (screenLock.IsHeld == true)
                { screenLock.Release();
                }
                
            });
            pm = null;
            screenLock = null;
        }
        /// <summary>
        /// This method awakes the screen when a new notification is posted, except when the app that
        /// publishes it, is Blacklisted by the user, so this notification has forbbiden to wake the device
        /// </summary>
        /// <param name="appPackage">the package of the app to check if it's blacklisted</param>
        public static void WakeUpScreenOnNewNotification(string appPackage)
        {
            //TODO: Check if App is blacklisted
        }
        /// <summary>
        /// This method will turn the screen off and lock the device at given time.
        /// Called by LockScreen and by Catcher
        /// </summary>
        public static void LockScreen()
        {
            //time in which the screen should remain On before turning it off.
            int timeToLockTheScreen = 5000;//TODO: a Value that can be user configurable

            //Make a thread which only acts as a Timer
            //By putting to sleep this thread and continue with the code below after <timeToLockScreen>)
            ThreadPool.QueueUserWorkItem(o =>
            {
                Thread.Sleep(timeToLockTheScreen);
            }
            );

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
                        //Should never happen, this setting is setted up on first launch of the app.
                        //TODO: This doesn't work, fix it man.
                        ex.ToString();
                        Console.WriteLine(ex);
                        Toast.MakeText(Application.Context, "Must enable dev admin", ToastLength.Long).Show();
                        ComponentName admin = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));
                        Intent intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin).PutExtra(DevicePolicyManager.ExtraDeviceAdmin, admin);
                        Application.Context.StartActivity(intent);
                    }
                }
            }
        }
    }
}
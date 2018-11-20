using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.OS;
using Android.Preferences;
using LiveDisplay.Misc;
using System;
using System.Threading;

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
    internal class Awake
    {
        //TODO: This class should Raise events instead of reacting to events!
        //The one that should react to events coming from this class is LockScreen.

        private static ISharedPreferences configurationManager = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

        /// <summary>
        /// Method that will wake up the screen when user picks up the phone from a plain surface
        /// ,user picks up the phone out of his pocket and when a new Notification is posted.
        /// called by CatcherService
        /// </summary>
        public static void WakeUpScreen()
        {
            //Only wake up the device if this setting is enabled.

            //TODO: LockScreen should implement this because, what if the Lockscreen is disabled? This method will turn on the screen anyways.
            //to show a notification, that is not available because the user maybe disabled the lockscreen.
            if (configurationManager.GetBoolean(ConfigurationParameters.turnonusermovement, false) == true)
            {
                PowerManager pm = ((PowerManager)Application.Context.GetSystemService(Context.PowerService));
                var screenLock = pm.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, "Turn On Lockscreen");
                screenLock.Acquire();
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Thread.Sleep(500);
                    if (screenLock.IsHeld == true)
                    {
                        screenLock.Release();
                    }
                });
            }
        }

        /// <summary>
        /// This method awakes the screen when a new notification is posted, except when the app that
        /// publishes it, is Blacklisted by the user, so this notification has forbbiden to wake the device
        /// </summary>
        public static void WakeUpScreenOnNewNotification()
        {

            PowerManager pm = ((PowerManager)Application.Context.GetSystemService(Context.PowerService));
            var screenLock = pm.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, "Turn On Lockscreen");
            screenLock.Acquire();
            ThreadPool.QueueUserWorkItem(o =>
            {
                Thread.Sleep(500);
                if (screenLock.IsHeld == true)
                {
                    screenLock.Release();
                }
            });
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
                        //Toast.MakeText(Application.Context, "Must enable dev admin", ToastLength.Long).Show();
                        //ComponentName admin = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));
                        //Intent intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin).PutExtra(DevicePolicyManager.ExtraDeviceAdmin, admin);
                        //Application.Context.StartActivity(intent);
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
                        //Toast.MakeText(Application.Context, "Must enable dev admin", ToastLength.Long).Show();
                        //ComponentName admin = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));
                        //Intent intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin).PutExtra(DevicePolicyManager.ExtraDeviceAdmin, admin);
                        //Application.Context.StartActivity(intent);
                    }
                }
            }
        }

        public static void TurnOffScreen()
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
                        //Toast.MakeText(Application.Context, "Must enable dev admin", ToastLength.Long).Show();
                        //ComponentName admin = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));
                        //Intent intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin).PutExtra(DevicePolicyManager.ExtraDeviceAdmin, admin);
                        //Application.Context.StartActivity(intent);
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
                        //Toast.MakeText(Application.Context, "Must enable dev admin", ToastLength.Long).Show();
                        //ComponentName admin = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));
                        //Intent intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin).PutExtra(DevicePolicyManager.ExtraDeviceAdmin, admin);
                        //Application.Context.StartActivity(intent);
                    }
                }
            }
        }
    }
}
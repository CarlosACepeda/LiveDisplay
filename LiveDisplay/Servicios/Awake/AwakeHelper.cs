using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.OS;
using Android.Util;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Notificaciones;
using System;
using System.Threading;

namespace LiveDisplay.Servicios.Awake
{
    public class AwakeHelper : Java.Lang.Object
    {
        private static ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        public AwakeHelper()
        {
            CatcherHelper.NotificationPosted += CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationListSizeChanged += CatcherHelper_NotificationListSizeChanged;
            AwakeService.DeviceIsActive += AwakeService_DeviceIsActive;
        }

        private void AwakeService_DeviceIsActive(object sender, EventArgs e)
        {
            TurnOnScreen();
        }

        public static void TurnOnScreen()
        {
            if (configurationManager.RetrieveAValue(ConfigurationParameters.TurnOnNewNotification) == true)
            {
                PowerManager pm = ((PowerManager)Application.Context.GetSystemService(Context.PowerService));
                var screenLock = pm.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, "Turn On Screen");
                if (AwakeService.isInPocket == false) //Dont wake up is the phone is inside a pocket.
                {
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
        }

        public static void TurnOffScreen()
        {
            PowerManager pm = (PowerManager)Application.Context.GetSystemService(Context.PowerService);
            DevicePolicyManager policy;
            if (Build.VERSION.SdkInt < BuildVersionCodes.KitkatWatch)
            {
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
                if (pm.IsScreenOn == true)
                {
                    policy = (DevicePolicyManager)Application.Context.GetSystemService(Context.DevicePolicyService);
                    try
                    {
                        policy.LockNow();
                    }
                    catch (Exception)
                    {
                        Log.Warn("LiveDisplay", "Lock device failed, check Device Admin permission");
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
                    catch (Exception)
                    {
                        Log.Warn("LiveDisplay", "Lock device failed, check Device Admin permission");
                    }
                }
            }
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
        }

        public bool IsAwakeListeningForDeviceOrientation()
        {
            if (AwakeService.GetAwakeStatus() == AwakeStatus.Up)
                return true;
            return false;
        }

        public bool IsAwakeActive()
        {
            //Check the current time and only react if the time this method is called is within the allowed hours.
            int start = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.StartSleepTime, "0")); //12am
            int end = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.FinishSleepTime, "500"));//5am
            //Generates the hour as a 4 characters number in 24 hours for example: 2210 (10:10pm)
            var now = int.Parse(string.Concat(DateTime.Now.Hour.ToString("00"), DateTime.Now.Minute.ToString("00")));
            Log.Info("LiveDisplay", now.ToString());

            if (start <= end) //The times are in the same day.
            {
                if (now >= start && now <= end)
                {
                    Log.Info("HELLO", "Im Sleeping");
                    return false;
                }
                else
                {
                    Log.Info("HELLO", "Im Active");
                    return true;
                }
            }
            else //The times are in different days.
            {
                if (now >= start || now <= end)
                {
                    Log.Info("HELLO", "Im Sleeping");
                    return false;
                }
                else
                {
                    Log.Info("HELLO", "Im Active");
                    return true;
                }
            }
        }

        private void CatcherHelper_NotificationListSizeChanged(object sender, Notificaciones.NotificationEventArgs.NotificationListSizeChangedEventArgs e)
        {
            if (configurationManager.RetrieveAValue(ConfigurationParameters.TurnOffScreenAfterLastNotificationCleared) == true)
            {
                if (e.ThereAreNotifications == false)
                    TurnOffScreen();
            }
        }

        private void CatcherHelper_NotificationPosted(object sender, Notificaciones.NotificationEventArgs.NotificationPostedEventArgs e)
        {
            if (e.ShouldCauseWakeUp)
                TurnOnScreen();
        }

        protected override void Dispose(bool disposing)
        {
            CatcherHelper.NotificationPosted -= CatcherHelper_NotificationPosted;
            CatcherHelper.NotificationListSizeChanged -= CatcherHelper_NotificationListSizeChanged;
            configurationManager.Dispose();
            base.Dispose(disposing);
        }
    }

    public enum AwakeStatus
    {
        Up = 1,
        Sleeping = 2,
        UpWithDeviceMotionDisabled = 4 //It can turn on the screen but not when grabbing the phone from a flat surface.
                                       //Maybe because the Service that listens for the device motion is not running.
    }
}
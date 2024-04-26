using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.OS;
using Android.Util;
using LiveDisplay.Adapters;
using LiveDisplay.Misc;
using LiveDisplay.Services.Keyguard;
using System;
using System.Threading;

namespace LiveDisplay.Services.Awake
{
    public class AwakeHelper : Java.Lang.Object
    {
        private static ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        public AwakeHelper()
        {
            NotificationAdapter.NotificationPosted += CatcherHelper_NotificationPosted;
            NotificationAdapter.NotificationListSizeChanged += Notification_Adapter_NotificationListSizeChanged;
        }

        public static void TurnOnScreen()
        {
            PowerManager pm = ((PowerManager)Application.Context.GetSystemService(Context.PowerService));
            var screenLock = pm.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, "Turn On Screen");
            screenLock.Acquire();
            ThreadPool.QueueUserWorkItem(o =>
            {
                Thread.Sleep(500);
                if (screenLock.IsHeld)
                {
                    screenLock.Release();
                }
            });
        }

        public static void TurnOffScreen()
        {
            if (Checkers.IsThisAppADeviceAdministrator())
            {
                PowerManager pm = (PowerManager)Application.Context.GetSystemService(Context.PowerService);
                DevicePolicyManager policy;
                if (Build.VERSION.SdkInt < BuildVersionCodes.KitkatWatch)
                {
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
                    if (pm.IsScreenOn)
                    {
                        policy = (DevicePolicyManager)Application.Context.GetSystemService(Context.DevicePolicyService);
                        try
                        {
                            policy.LockNow();
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("LiveDisplay", "Lock device failed, check Device Admin permission: " + ex);
                        }
                    }
                }
                else
                {
                    if (pm.IsInteractive)
                    {
                        policy = (DevicePolicyManager)Application.Context.GetSystemService(Context.DevicePolicyService);
                        try
                        {
                            //Special workaround that still makes harm but not as much as it would without the workaround.
                            //So, if the next line of code gets triggered 'policy.LockNow()' it would lock the device, but, if
                            //the device has been locked when the user has set a fingerprint and also if the lockscreen is still present
                            //then after unlocking by using the fingerprint the user has to write
                            //the pattern, aaand also double tap to exit LiveDisplay, lol.
                            //this workaround is to prevent that, when I don't call policy.LockNow() the user doesn't have to write the pattern
                            //or whatever side security method they have set along the fingerprint, the cost of this btw is that double tap won't work
                            //neither the automatic screen off. (I don't know how to solve that yet) it is the lesser of two evils.
                            //However it doesn't work sometimes, I guess it is better to simply warn the user about it.
                            //And disable the turn off screen capabilities of LiveDisplay while a fingerprint lock is active.
                            if (KeyguardHelper.IsDeviceCurrentlyLocked() && KeyguardHelper.IsFingerprintSet())
                            {
                                //Do nothing.
                            }
                            else
                                policy.LockNow();
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("LiveDisplay", "Lock device failed, check Device Admin permission: " + ex);
                        }
                    }
                }
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
            }
            else
            {
                Log.Info("LiveDisplay", "Ignoring request to turn off screen, we don't have permission");
            }
        }

        public static void ToggleStartStopAwakeService(bool toggle)
        {
            if (toggle)
            {
                Intent awake = new Intent(Application.Context, typeof(AwakeService));
                Application.Context.StartService(awake);
            }
            else
            {
                Intent awake = new Intent(Application.Context, typeof(AwakeService));
                Application.Context.StopService(awake);
            }
        }

        public static AwakeStatus GetAwakeStatus()
        {
            //if (UserHasEnabledAwake() == false && UserHasSetAwakeHours() == false)
            //    return AwakeStatus.CompletelyDisabled;
            //if (UserHasEnabledAwake() == false && UserHasSetAwakeHours())
            //    return AwakeStatus.DisabledbyUser;
            //if (UserHasEnabledAwake() && IsAwakeUp() && AwakeService.isRunning)
            //    return AwakeStatus.Up;
            //if (UserHasEnabledAwake() && IsAwakeUp() && AwakeService.isRunning == false)
            //    return AwakeStatus.UpWithDeviceMotionDisabled;
            //if (UserHasEnabledAwake() && IsAwakeUp() == false && AwakeService.isRunning)
            //    return AwakeStatus.SleepingWithDeviceMotionEnabled;
            //if (UserHasEnabledAwake() && IsAwakeUp() == false && AwakeService.isRunning == false)
            //    return AwakeStatus.Sleeping;

            return AwakeStatus.None;
        }

        private static bool IsAwakeUp()
        {
            //Check the current time and only react if the time this method is called is within the allowed hours.
            int start = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.StartSleepTime, "-1"));
            int end = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.FinishSleepTime, "-1"));
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

        private static bool UserHasEnabledAwake()
        {
            //Check if the user has enabled it in the first place
            if (!configurationManager.RetrieveAValue(ConfigurationParameters.ListenForDeviceMotion))
            {
                return false;
            }
            return true;
        }

        public static bool UserHasSetAwakeHours()
        {
            //Check if the user has set  hours in which the Awake functionality isn't working!
            int start = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.StartSleepTime, "-1"));
            int end = int.Parse(configurationManager.RetrieveAValue(ConfigurationParameters.FinishSleepTime, "-1"));
            if (start == -1 || end == -1)
            {
                return false;
            }
            return true;
        }

        private void Notification_Adapter_NotificationListSizeChanged(object sender, Notifications.NotificationEventArgs.NotificationListSizeChangedEventArgs e)
        {
            if (configurationManager.RetrieveAValue(ConfigurationParameters.TurnOffScreenAfterLastNotificationCleared) && !e.ThereAreNotifications)
            {
                TurnOffScreen();
            }
        }

        private void CatcherHelper_NotificationPosted(object sender, Notifications.NotificationEventArgs.NotificationPostedEventArgs e)
        {
            if (e.ShouldCauseWakeUp)
                TurnOnScreen();
        }

        protected override void Dispose(bool disposing)
        {
            NotificationAdapter.NotificationPosted -= CatcherHelper_NotificationPosted;
            NotificationAdapter.NotificationListSizeChanged -= Notification_Adapter_NotificationListSizeChanged;
            configurationManager.Dispose();
            base.Dispose(disposing);
        }
    }

    public enum AwakeStatus
    {
        None = -1, //Shrug.
        CompletelyDisabled = 0, //Not even enabled by the user yet.
        Up = 1,
        Sleeping = 2, //Enabled but currently inactive! (Inactive hours)
        UpWithDeviceMotionDisabled = 4, //It can turn on the screen but not when grabbing the phone from a flat surface.

                                        //Maybe because the Service that listens for the device motion is not running.
        SleepingWithDeviceMotionEnabled = 8,

        DisabledbyUser = 64
    }
}
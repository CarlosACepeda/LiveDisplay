using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Util;
using LiveDisplay.Misc;
using System;
using System.Threading;

namespace LiveDisplay.Servicios
{
    internal class Awake
    {

        private static ISharedPreferences configurationManager = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

        public static void WakeUpScreen()
        {
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
        }
    }
}
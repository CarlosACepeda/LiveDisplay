using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Misc;
using System;
using System.Threading;

namespace LiveDisplay.Servicios
{
    [Service(Label = "Awake")]
    internal class Awake : Service, ISensorEventListener
    {
        private static ISharedPreferences configurationManager = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        private SensorManager sensorManager;
        private Sensor sensor;
        private bool isLaidDown = false;
        private static int currentstartsleeptime = int.Parse(configurationManager.GetString(ConfigurationParameters.StartSleepTime, "0")); //12am
        private static int currentfinishsleeptime = int.Parse(configurationManager.GetString(ConfigurationParameters.FinishSleepTime, "500"));//5am
        public static void WakeUpScreen()
        {
            //Check the current time and only react if the time this method is called is within the allowed hours.

            //Generates the hour as a 4 characters number in 24 hours for example: 2210 (10:10pm)
            var currenttime = int.Parse(string.Concat(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()));

            if(currentfinishsleeptime < currenttime && currenttime < currentstartsleeptime)
            if (configurationManager.GetBoolean(ConfigurationParameters.TurnOnNewNotification, false) == true|| configurationManager.GetBoolean(ConfigurationParameters.TurnOnUserMovement, false) == true)
            {
                PowerManager pm = ((PowerManager)Application.Context.GetSystemService(PowerService));
                var screenLock = pm.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, "Turn On Screen");
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

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            sensorManager = GetSystemService(SensorService) as SensorManager;

            sensor = sensorManager.GetDefaultSensor(SensorType.Accelerometer);

            sensorManager.RegisterListener(this, sensor, SensorDelay.Normal);

            return StartCommandResult.Sticky;
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (e.Sensor.Type == SensorType.Accelerometer)
            {
                //Detect phone on plain surface:
                //Z axis must have the following value:
                //>10 m/s2;
                //Y axis must be less than 3m/s2 so, the device can be slightly tilted and still being
                //in a Plain surface.

                if (e.Values[2] > 10 && e.Values[1] < 3)
                {
                    isLaidDown = true;
                }
                //after, use this value to decide if wake up or not the screen.
                //We don't want to awake the screen if the device is already vertical for some reason.

                //Put a timer of 3 seconds, and if the device is still with these values,
                //the phone is left in a plain surface.
                //New feature? Don't awake phone on new Notification while phone is left alone
                //To avoid Unnecesary awake if the user won't see it.

                //Detect if User has grabbed the phone back up:
                //Z axis must be less than 10 m/s2("Example: 9.5") it means that Z  axis is not being
                //Accelerated and
                //Y axis must be greater than 3m/s20
                else if (ScreenOnOffReceiver.IsScreenOn == false && isLaidDown == true)
                {
                    if (e.Values[2] < 9.6f && e.Values[1] > 3)
                    {
                        //Awake the phone:
                        WakeUpScreen();
                        isLaidDown = false;
                    }
                    else
                    {
                        isLaidDown = true;
                    }
                }
            }

            //The less Z axis m/s2 value is, and the more Y axis m/s2 value is, the phone more vertically is.

            //Notes:
            //X axis is not necessary as I don't need to know if the phone is being moved Horizontally.
        }

        public override void OnDestroy()
        {
            sensorManager.UnregisterListener(this);
            sensor.Dispose();
            sensorManager.Dispose();
            base.OnDestroy();
        }
    }
}
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Hardware;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Awake;
using System;

namespace LiveDisplay.Servicios
{
    [Service(Label = "MotionListener")]
    internal class AwakeService : Service, ISensorEventListener
    {
        private static ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
        private SensorManager sensorManager;
        private Sensor accelerometerSensor;
        private Sensor proximitySensor;
        public static bool isLaidDown = false;
        public static bool isInPocket;
        public static bool isRunning;

        

        public static event EventHandler<EventArgs> DeviceIsActive;


        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            sensorManager = GetSystemService(SensorService) as SensorManager;

            accelerometerSensor = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
            proximitySensor = sensorManager.GetDefaultSensor(SensorType.Proximity);
            sensorManager.RegisterListener(this, accelerometerSensor, SensorDelay.Normal);
            sensorManager.RegisterListener(this, proximitySensor, SensorDelay.Normal);
            isRunning = true;
            return StartCommandResult.Sticky;
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            switch (e.Sensor.Type)
            {
                case SensorType.Accelerometer:
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
                    //We don't want to turn on the screen if the device is already vertical for some reason.

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
                            DeviceIsActive?.Invoke(null, null);
                            isLaidDown = false;
                        }
                        else
                        {
                            isLaidDown = true;
                        }
                    }

                    //The less Z axis m/s2 value is, and the more Y axis m/s2 value is, the phone more vertically is.

                    //Notes:
                    //X axis is not necessary as I don't need to know if the phone is being moved Horizontally.

                    break;

                case SensorType.Proximity:
                    Log.Info("Livedisplay", "value 1 " + e.Values[0]);
                    if (e.Values[0] == 0)
                    {
                        //Phone is in front of something or something is blocking the sensor.
                        if (isLaidDown == false) //We need to check if the phone is vertical enough and the proximity sensor covered
                                                 //To assume is in a pocket.
                            isInPocket = true;
                        else
                        {
                            isInPocket = false; //Nothing is blocking the sensor, and I bet there arent ghost pockets without tangible
                            //boundaries so the Proximity sensor does not detect anything. xD
                        }
                    }
                    else //The sensor value is a different value but I will just assume the phone prox. Sensor is not being blocked.
                    {
                        isInPocket = false;
                    }

                    break;

                default:
                    break;
            }
            //Kind of works. :/
            //If you turn off the screen when the phone is vertical then okay, the screen won't turn on, but if you lay down your phone
            //and get it back up it won't turn on the screen again.
            //Because the code doesn't recognize that I wan't to turn on the screen and that the phone is no longer vertical.
            //if I remove the third argument then when I turn off the Screen it will immediately turn on the screen
            //because it recognizes is vertical and also it isn't inside a pocket
            //TODO: Fix this behavior

            if (isInPocket == false && isLaidDown == false && ScreenOnOffReceiver.ScreenTurnedOffWhileInVertical==false)
            {
                if (configurationManager.RetrieveAValue(ConfigurationParameters.TurnOnUserMovement) == true)
                {
                    AwakeHelper.TurnOnScreen();
                }
            }
        }

        public override void OnDestroy()
        {
            sensorManager.UnregisterListener(this);
            accelerometerSensor.Dispose();
            sensorManager.Dispose();
            isRunning = false;
            base.OnDestroy();
        }
    }
}
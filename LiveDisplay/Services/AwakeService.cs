using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Util;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Awake;

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
        private static bool isSensorBlocked;
        public static bool isRunning;
        private long layDownTime; //Timestamp indicating at which moment the device is laid down.
        private long wakeUpWaitTime = 500000000; //Half a second in nanoseconds.
        private long sleepTime = 1000000000; //1 Seconds in nanoseconds.
        private long phoneInVerticalTime; //Timestamp indicating at which moment the device is vertical.
        private long proxSensorBlockedTime; //Timestamp indicating at which moment the proximity sensor is being blocked;

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

                    if (e.Values[2] > 9 && e.Values[1] < 3)
                    {
                        if (layDownTime == 0)
                        {
                            layDownTime = e.Timestamp;
                        }

                        if (e.Timestamp > (sleepTime + layDownTime))
                        {
                            isLaidDown = true;
                            Log.Info("SENSOR", "Is Laid down");
                            if (isSensorBlocked)
                            {
                                isInPocket = true;
                                Log.Info("SENSOR", "Is in Pocket (horizontal)");
                            }
                            else
                            {
                                Log.Info("SENSOR", "Is not in Pocket (horizontal)");

                                isInPocket = false;
                            }
                        }
                        phoneInVerticalTime = 0;
                    }
                    if (e.Values[1] > 3 && isInPocket == false)
                    {
                        if (isLaidDown == true)
                        {
                            if (phoneInVerticalTime == 0)
                            {
                                phoneInVerticalTime = e.Timestamp;
                            }
                            if (e.Timestamp > (wakeUpWaitTime + phoneInVerticalTime))
                            {
                                if (ScreenOnOffReceiver.IsScreenOn == false)
                                {
                                    Log.Info("SENSOR", "Should turn on screen");
                                    if (configurationManager.RetrieveAValue(ConfigurationParameters.TurnOnUserMovement))
                                    {
                                        AwakeHelper.TurnOnScreen();
                                    }
                                }
                                isLaidDown = false;
                                layDownTime = 0;
                            }
                        }
                        else if (isLaidDown == false && e.Timestamp > (phoneInVerticalTime + sleepTime) && isSensorBlocked)
                        {
                            isInPocket = true;
                            Log.Info("SENSOR", "Is In Pocket (vertical) I guess");
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
                        proxSensorBlockedTime = e.Timestamp;
                        isSensorBlocked = true;
                    }
                    else
                    {
                        proxSensorBlockedTime = 0;
                        isSensorBlocked = false;
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

            //if (isInPocket == false && isLaidDown == false && ScreenOnOffReceiver.ScreenTurnedOffWhileInVertical==false)
            //{
            //    if (configurationManager.RetrieveAValue(ConfigurationParameters.TurnOnUserMovement) == true)
            //    {
            //        AwakeHelper.TurnOnScreen();
            //    }
            //}
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
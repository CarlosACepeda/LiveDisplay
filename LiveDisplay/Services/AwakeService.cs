using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Util;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Misc;
using LiveDisplay.Services.Awake;
using System;
using System.Collections.Generic;

namespace LiveDisplay.Services
{
    [Service(Label = "MotionListener")]
    internal class AwakeService : Service, ISensorEventListener
    {
        private static ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
        private SensorManager sensorManager;
        private Sensor accelerometerSensor;
        private Sensor proximitySensor;
        private Sensor lightSensor;
        private float yAbs;
        private double norm_Of_g_last;
        private double acceleration;
        private int inclination;
        private float proximity;
        private float light;
        private IList<float> g;
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
            lightSensor = sensorManager.GetDefaultSensor(SensorType.Light);
            sensorManager.RegisterListener(this, accelerometerSensor, SensorDelay.Normal);
            sensorManager.RegisterListener(this, proximitySensor, SensorDelay.Normal);
            sensorManager.RegisterListener(this, lightSensor, SensorDelay.Normal);
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
                    g = e.Values;

                    double norm_Of_g = Math.Sqrt(g[0] * g[0] + g[1] * g[1] + g[2] * g[2]);

                    g[0] = (float)(g[0] / norm_Of_g);
                    g[1] = (float)(g[1] / norm_Of_g);
                    g[2] = (float)(g[2] / norm_Of_g);

                    yAbs = Math.Abs(g[1]);

                    norm_Of_g_last = norm_Of_g;
                    double delta = norm_Of_g - norm_Of_g_last;

                    acceleration = acceleration * 0.9 + delta;

                    inclination = (int)Math.Round(Java.Lang.Math.ToDegrees(Math.Acos(g[2])));

                    break;
                case SensorType.Proximity:
                    proximity = e.Values[0];
                    break;

                case SensorType.Light:
                    light = e.Values[0];
                    break;
                default:
                    break;
            }

            if (IsInPocket(proximity, light, g, inclination) && ScreenOnOffReceiver.IsScreenOn)
            {
                AwakeHelper.TurnOffScreen();
            }
        }
        bool IsInPocket(float prox, float light, IList<float> g, int inc)
        {
            if ((prox < 1) && (light < 60) && g != null && (g[1] < -0.6  || g[1] > 0.8) && ((inc > 75) || (inc < 110)))
                {
                    return true;
                }
                if ((prox >= 1) && (light >= 2) && g!= null && g[1] >= -0.7)
                {
                    return false;
                }
            return false;
        }
        bool IsPickedUp(float yAccelAbs)
        {
            Console.WriteLine(yAccelAbs);
            if (yAccelAbs > 0.25 && yAccelAbs < 0.6)
            {
                return true;
            }
            return false;
        }
        public override void OnDestroy()
        {
            sensorManager.UnregisterListener(this);
            accelerometerSensor.Dispose();
            proximitySensor.Dispose();
            lightSensor.Dispose();
            sensorManager.Dispose();

            base.OnDestroy();
        }
    }
}
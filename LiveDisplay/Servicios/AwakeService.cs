using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Widget;

namespace LiveDisplay.Servicios
{
    [Service(Name = "undergrounddev.serv.AwakeService", Label = "Awake Service")]
    internal class AwakeService : Service, ISensorEventListener
    {
        private SensorManager sensorManager = null;
        private Sensor sProximidad;
        private Sensor sAcelerometro;
        private PowerManager powerManager = null;
        private PowerManager.WakeLock wakeLock = null;


        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            IniciarVariables();
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            //Desregistrar el Listener.
            sensorManager.UnregisterListener(this);
            Toast.MakeText(this, "El servicio ha sido detenido", ToastLength.Short).Show();
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            //No hay nada para hacer aquí
        }

        public void OnSensorChanged(SensorEvent evento)
        {
            //Arreglame: Ejecutame en un Backgroudn thread
            Sensor sensor = evento.Sensor;

            if (sensor.Type == SensorType.Proximity)
            {
                //Mientras el sensor de proximidad esté cubierto haga:
                while (evento.Values[0] > 1)
                {
                    //Si el wakelock no está sostenido, entonces adquirir el Wakelock que apaga la pantalla.
                    if (!wakeLock.IsHeld)
                    {
                        wakeLock.Acquire();
                    }
                }
                //Si el wakelock está adquirido, entonces soltarlo cuando el sensor no esté cubierto.
                if (wakeLock.IsHeld)
                {
                    wakeLock.Release();
                }
            }
            else if (sensor.Type == SensorType.Accelerometer)
            {
                //values 1 es el eje Y, values 2 es el eje Z
                while (evento.Values[1] > 3 && evento.Values[2] < 9)
                {
                    Toast.MakeText(Application.Context, "El dispositivo se levantó", ToastLength.Short).Show();
                }
            }
        }

        private void IniciarVariables()
        {
            powerManager = (PowerManager)GetSystemService(PowerService);
            //Wakelock que apaga la pantalla.
            wakeLock = powerManager.NewWakeLock(WakeLockFlags.ProximityScreenOff, "Mi Etiqueta");
            sensorManager = (SensorManager)GetSystemService(SensorService);
            sProximidad = sensorManager.GetDefaultSensor(SensorType.Proximity);
            sAcelerometro = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
            //Registrar un Listener para Oir los eventos de los sensores previamente obtenidos-
            sensorManager.RegisterListener(this, sProximidad, SensorDelay.Fastest);
            sensorManager.RegisterListener(this, sAcelerometro, SensorDelay.Normal);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Hardware;

namespace LiveDisplay
{
    [Service(Name = "undergrounddev.serv.AwakeService",
        Label = "Awake Service")]
    class AwakeService : Service, ISensorEventListener
    {
        //administra los sensores de Android.
        SensorManager sensorManager = null;
        //Variable la cuál guardará la constante del Sensor de Proximidad.
        Sensor sProximidad;
        //administra la Energia de Android.
        PowerManager powerManager = null;
        //Un Wakelock que servirá para prender y apagar la pantalla.
        PowerManager.WakeLock wakeLock = null;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            //Cuando se inicia

            //Asignación de variables, sensorManager al Servicio SensorService, sProximidad al sensor SensorType.Proximity
            //powerManager al servicio PowerManager
            powerManager = (PowerManager)GetSystemService(PowerService);
            //Wakelock que apaga la pantalla.
            wakeLock = powerManager.NewWakeLock(WakeLockFlags.ProximityScreenOff, "Mi Etiqueta");
            sensorManager = (SensorManager)GetSystemService(SensorService);
            sProximidad = sensorManager.GetDefaultSensor(SensorType.Proximity);
            //Registrar un Listener para Oir los eventos del Sensor sProximidad.
            sensorManager.RegisterListener(this, sProximidad, SensorDelay.Fastest);
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            //Acá el servicio termina.
            //Desregistrar el Listener.
            sensorManager.UnregisterListener(this);
            Toast.MakeText(this, "El servicio ha sido detenido", ToastLength.Short).Show();
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            //No hay nada para hacer aquí
        }

        //..Cuando detecta un cambio en el sensor.
        public void OnSensorChanged(SensorEvent evento)
        {
            //Mientras el sensor esté cubierto haga:
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
    }
}

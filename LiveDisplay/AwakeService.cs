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
    [Service(Name = "mono.samples.HelloService",
        Label = "Service Label")]
    class AwakeService : Service, ISensorEventListener
    {
        //administra los sensores de Android.
        SensorManager sensorManager = null;
        //Variable la cuál guardará la constante del Sensor de Proximidad.
        Sensor sProximidad;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            //Cuando se inicia
            //Asignación de variables, sensorManager al Servicio SensorService, sProximidad al sensor SensorType.Proximity
            sensorManager = (SensorManager)GetSystemService(SensorService);
            sProximidad = sensorManager.GetDefaultSensor(SensorType.Proximity);
            //Registrar un Listener para Oir los eventos del Sensor sProximidad.
            sensorManager.RegisterListener(this, sProximidad, SensorDelay.Fastest);
            Toast.MakeText(this, "El servicio ha sido iniciado", ToastLength.Short).Show();
         

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            //Acá el servicio termina.
            //Desregistrar el Listener.
            sensorManager.UnregisterListener(this);
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            //Nada todavía.
        }

        //..Cuando detecta un cambio en el sensor.
        public void OnSensorChanged(SensorEvent evento)
        {
            if (evento.Values[0] > 1)
            {
                Toast.MakeText(this, "El sensor detecta proximidad", ToastLength.Short).Show();
            }
            else if (evento.Values[0] <= 1)
            {
                Toast.MakeText(this, "Algo sucede...", ToastLength.Long).Show();
            }
        }

        //public void ToastService()
        //{
        //    if (estaProximo == true)
        //    {
                
        //    }
        //    else if (estaProximo == false)
        //    {
               
        //    }

        //}

    }
}
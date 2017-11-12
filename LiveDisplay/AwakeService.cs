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

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            //Cuando se inicia
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
            //Nada todavía.
        }

        //..Cuando detecta un cambio en el sensor.
        public void OnSensorChanged(SensorEvent evento)
        {
            ////Mientras el sensor esté cubierto haga:
            while (evento.Values[0] > 1)
            {
               //<Apagar la pantalla> TODO
            }
            

        }
        public void SetBright(float value)
        {
            //Este método se llama para apagar la pantalla.
        }
    }
}

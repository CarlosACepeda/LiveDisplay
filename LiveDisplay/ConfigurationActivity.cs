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

namespace LiveDisplay
{
    [Activity(Label = "Configuración", MainLauncher = true)]
    class ConfigurationActivity: Activity
    {
        //Android Lifecycle.
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Configuracion);
            CheckBox cbxEnableAwake = FindViewById<CheckBox>(Resource.Id.cbxEnableAwake);
            //O es para Objeto, e es para Evento.
            cbxEnableAwake.Click += (o, e) =>
            {
                if (cbxEnableAwake.Checked == true)
                {
                    StartService(new Intent(this, typeof(AwakeService)));
                }
                else if (cbxEnableAwake.Checked == false)
                {
                    StopService(new Intent(this, typeof(AwakeService)));
                }
            };
        }
      
        protected override void OnPause()
        {
            base.OnPause();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        
    }
}

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using LiveDisplay.Servicios;

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
        public override bool OnCreateOptionsMenu(IMenu menu)
        {

            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_config, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.preview:
                    {
                        Intent intent = new Intent(this, typeof(LockScreenActivity));
                        StartActivity(intent);
                        return true;
                    }

                case Resource.Id.notificationSettings:
                    {
                        string lel = Android.Provider.Settings.ActionNotificationListenerSettings;
                        StartActivity(new Intent(lel));
                        return true;
                    }
            }
            return true;
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
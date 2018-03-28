using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
using System;
using System.Threading;

//for CI.
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace LiveDisplay
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    internal class ConfigurationActivity : AppCompatActivity
    {
        SwitchCompat swEnableAwake;
        Android.Support.V7.Widget.Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Configuracion);         
        }
        protected override void OnResume()
        {
            base.OnResume();
            BindViews();
            StartVariables();
            
        }
        protected override void OnPause()
        {
            base.OnPause();
            UnbindViews();
            GC.Collect();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
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
                        if (NLChecker.IsNotificationListenerEnabled() == true)
                        {
                            Intent intent = new Intent(this, typeof(LockScreenActivity));
                            intent.AddFlags(ActivityFlags.NewDocument);
                            StartActivity(intent);

                            intent = null;
                            return true;
                        }
                        else
                        {
                            Toast.MakeText(ApplicationContext, "Listener desconectado, no puedes", ToastLength.Short).Show();
                            return false;
                        }
                        
                    }

                case Resource.Id.notificationSettings:
                    {
                        string lel = Android.Provider.Settings.ActionNotificationListenerSettings;
                        StartActivity(new Intent(lel).AddFlags(ActivityFlags.NewTask));
                        lel = null;
                        return true;
                    }
            }
            return true;
        }        
        protected void UnbindViews()
        {
            swEnableAwake = null;
            toolbar = null;
            Window.ClearFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
        }
        protected void UnbindClickEvents()
        {
            //Nothing yet.
        }
        protected void BindViews()
        {
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.my_toolbar);
            swEnableAwake = FindViewById<SwitchCompat>(Resource.Id.swEnableAwake);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            }
            SetSupportActionBar(toolbar);
        }
        private void StartVariables()
        {
            //CI
            AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa",
                   typeof(Analytics), typeof(Crashes));
            AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa", typeof(Analytics), typeof(Crashes));
            //O es para Objeto, e es para Evento.
            swEnableAwake.Click += (o, e) =>
            {
                if (swEnableAwake.Checked == true)
                {
                    //StartService(new Intent(this, typeof(AwakeService)));
                    Toast.MakeText(this, Resource.String.idk, ToastLength.Short).Show();
                }
                else if (swEnableAwake.Checked == false)
                {
                    Toast.MakeText(this, Resource.String.idk, ToastLength.Short).Show();
                    //StopService(new Intent(this, typeof(AwakeService)));
                }
            };
        }
    }
}
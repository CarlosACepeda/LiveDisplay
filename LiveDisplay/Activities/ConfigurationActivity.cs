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
using LiveDisplay.Servicios;

namespace LiveDisplay
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/ic_launcher_2_dark")]
    internal class ConfigurationActivity : AppCompatActivity
    {
        Android.Support.V7.Widget.Toolbar toolbar;
        SwitchCompat swEnableAwake;
        SwitchCompat swEnableLockScreen;
        SwitchCompat swEnableDynamicWallpaper;
        SwitchCompat swHideSystemIcons;
        
        
        ConfigurationManager configurationManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Configuracion);

            configurationManager = new ConfigurationManager(GetPreferences(FileCreationMode.Private));
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
            swEnableLockScreen = null;
            swEnableDynamicWallpaper = null;
            swHideSystemIcons = null;
            toolbar = null;
            Window.ClearFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
        }
        protected void UnbindClickEvents()
        {
            //Nothing yet.
        }
        protected void BindViews()
        {
            //Needed for Status Bar Tinting on Lollipop and beyond using AppCompat
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            }
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.my_toolbar);
            SetSupportActionBar(toolbar);
            swEnableAwake = FindViewById<SwitchCompat>(Resource.Id.swEnableAwake);
            swEnableLockScreen = FindViewById<SwitchCompat>(Resource.Id.swEnableLockscreen);
            swEnableDynamicWallpaper= FindViewById<SwitchCompat>(Resource.Id.swEnableDynamicWallpaper);
            swHideSystemIcons= FindViewById<SwitchCompat>(Resource.Id.swHideSystemIcons);
           
        }
        private void StartVariables()
        {
            //CI
            AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa",
                   typeof(Analytics), typeof(Crashes));
            AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa", typeof(Analytics), typeof(Crashes));
            //sender es para Objeto, e es para Evento.
            swEnableAwake.CheckedChange += (sender, e) =>
            {
                if (swEnableAwake.Checked == true)
                {
                    //StartService(new Intent(this, typeof(AwakeService)));
                    Toast.MakeText(this, Resource.String.idk, ToastLength.Short).Show();
                    configurationManager.SaveAValue("awakeEnabled", true);


                }
                else if (swEnableAwake.Checked == false)
                {
                    Toast.MakeText(this, Resource.String.idk, ToastLength.Short).Show();
                    configurationManager.SaveAValue("awakeEnabled", false);
                    //StopService(new Intent(this, typeof(AwakeService)));
                }
            };
            swEnableLockScreen.CheckedChange += (sender, e) =>
             {
                 if (swEnableLockScreen.Checked==true)
                 {
                     if (NLChecker.IsNotificationListenerEnabled() == false)
                     {
                         Android.Support.V7.App.AlertDialog.Builder dialogBuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
                         dialogBuilder.SetMessage("Debes permitir que esta app acceda a las notificaciones primero!");//Add this to strings.xml
                         dialogBuilder.SetPositiveButton("Activar", new EventHandler<DialogClickEventArgs>(OnDialogClickPositiveEventArgs));
                         dialogBuilder.SetNegativeButton("Cancelar", new EventHandler<DialogClickEventArgs>(OnDialogClickNegativeEventArgs));
                         dialogBuilder.Show();
                         swEnableLockScreen.Checked = false;
                     }
                     else
                     {
                         configurationManager.SaveAValue("lockScreenEnabled", true);
                     }
                 }
             };

        }
        private void OnDialogClickPositiveEventArgs(object sender, DialogClickEventArgs e)
        {
            string lel = Android.Provider.Settings.ActionNotificationListenerSettings;
            Intent intent = new Intent(lel).AddFlags(ActivityFlags.NewTask);
            StartActivity(intent);
        }
        private void OnDialogClickNegativeEventArgs(object sender, DialogClickEventArgs e)
        {
            //Nada.
        }
    }
}
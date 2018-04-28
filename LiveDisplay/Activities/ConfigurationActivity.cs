using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using LiveDisplay.Activities;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;

//for CI.
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;

namespace LiveDisplay
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/ic_launcher_2_dark")]
    internal class ConfigurationActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;

        private Button btnLockScreen;
        private Button btnNotifications;
        private Button btnAwake;
        private Button btnAbout;

        private ConfigurationManager configurationManager;

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
                            Finish();
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
            toolbar = null;
            Window.ClearFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            configurationManager = null;
            btnAbout = null;
            btnAwake = null;
            btnLockScreen = null;
            btnNotifications = null;
        }

        protected void UnbindClickEvents()
        {
        }

        protected void BindViews()
        {
            //Needed for Status Bar Tinting on Lollipop and beyond using AppCompat
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            }
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.mainToolbar);
            SetSupportActionBar(toolbar);

            btnLockScreen = FindViewById<Button>(Resource.Id.btnLockScreen);
            btnNotifications = FindViewById<Button>(Resource.Id.btnNotification);
            btnAwake = FindViewById<Button>(Resource.Id.btnAwake);
            btnAbout = FindViewById<Button>(Resource.Id.btnAbout);
        }

        private void StartVariables()
        {
            //CI
            AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa",
                   typeof(Analytics), typeof(Crashes));
            AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa", typeof(Analytics), typeof(Crashes));

            btnLockScreen.Click += (o, e) => StartActivity(new Intent(this, typeof(LockScreenSettingsActivity)).AddFlags(ActivityFlags.ClearTop));
            btnNotifications.Click += (o, e) => StartActivity(new Intent(this, typeof(NotificationSettingsActivity)).AddFlags(ActivityFlags.ClearTop));
            btnAwake.Click += (o, e) => StartActivity(new Intent(this, typeof(AwakeSettingsActivity)).AddFlags(ActivityFlags.ClearTop));
            btnAbout.Click += (o, e) => StartActivity(new Intent(this, typeof(AboutActivity)).AddFlags(ActivityFlags.ClearTop));
        }

        private void OnDialogClickPositiveEventArgs(object sender, DialogClickEventArgs e)
        {
            string lel = Android.Provider.Settings.ActionNotificationListenerSettings;
            Intent intent = new Intent(lel).AddFlags(ActivityFlags.NewTask);
            StartActivity(intent);
            intent = null;
        }

        private void OnDialogClickNegativeEventArgs(object sender, DialogClickEventArgs e)
        {
            //Nada.
        }
    }
}
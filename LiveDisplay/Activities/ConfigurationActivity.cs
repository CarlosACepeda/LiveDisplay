using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Activities;
using LiveDisplay.BroadcastReceivers;
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
            BindViews();
            StartVariables();
            configurationManager = new ConfigurationManager(GetSharedPreferences("livedisplayconfig", FileCreationMode.Private));
            if (configurationManager.RetrieveAValue(ConfigurationParameters.istheappconfigured)==false)
            {
                LoadDefaultConfiguration();
            }
            ShowDialog();
            

        }

        private void LoadDefaultConfiguration()
        {
            configurationManager.SaveAValue(ConfigurationParameters.istheappconfigured, true);
        }

        protected override void OnResume()
        {
            base.OnResume();
            

        }

        protected override void OnPause()
        {
            base.OnPause();
            UnbindClickEvents();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnbindViews();
            //Workaround, Memory is never released.
            GC.Collect();
        }




        protected void UnbindViews()
        {
            toolbar.Dispose();
            Window.ClearFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            btnAbout.Dispose();
            btnAwake.Dispose();
            btnLockScreen.Dispose();
            btnNotifications.Dispose();
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
            ////CI
            AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa",
                   typeof(Analytics), typeof(Crashes));
            AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa", typeof(Analytics), typeof(Crashes));

            btnLockScreen.Click += (o, e) => StartActivity(new Intent(this, typeof(LockScreenSettingsActivity)).AddFlags(ActivityFlags.ClearTop));
            btnNotifications.Click += (o, e) => StartActivity(new Intent(this, typeof(NotificationSettingsActivity)).AddFlags(ActivityFlags.ClearTop));
            btnAbout.Click += (o, e) => StartActivity(new Intent(this, typeof(AboutActivity)).AddFlags(ActivityFlags.ClearTop));
        }

        private void ShowDialog()
        {
        if (NLChecker.IsNotificationListenerEnabled() == false)
        {
            //Prompt a message to go to NotificationListenerService.
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetMessage(Resource.String.dialognldisabledmessage);
            builder.SetPositiveButton(Resource.String.dialognldisabledpositivebutton, new EventHandler<DialogClickEventArgs>(OnDialogPositiveButtonEventArgs));
            builder.SetNegativeButton(Resource.String.dialognldisablednegativebutton, new EventHandler<DialogClickEventArgs>(OnDialogNegativeButtonEventArgs));
            builder.Show();
        }
        }
        private void OnDialogNegativeButtonEventArgs(object sender, DialogClickEventArgs e)
        {
            Toast.MakeText(this, Resource.String.dialogcancelledclick, ToastLength.Long).Show();
        }
        private void OnDialogPositiveButtonEventArgs(object sender, DialogClickEventArgs e)
        {
            string lel = Android.Provider.Settings.ActionNotificationListenerSettings;
            Intent intent = new Intent(lel).AddFlags(ActivityFlags.NewTask);
            StartActivity(intent);
            intent = null;
        }
    }
}
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
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
using System.Threading;

namespace LiveDisplay.Activities
{
    //THis will be MainActivity
    [Activity(Label = "@string/app_name", Theme ="@style/LiveDisplayTheme.NoActionBar",  MainLauncher = true)]
    internal class MainActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;
        private ConfigurationManager configurationManager;
        ISharedPreferences sharedPreferences;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            BindViews();
            StartVariables();
            using (sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this))
            {
                configurationManager = new ConfigurationManager(sharedPreferences);
                if (sharedPreferences.GetBoolean(ConfigurationParameters.istheappconfigured, false) == false)
                {
                    LoadDefaultConfiguration();
                }
                configurationManager.Dispose();
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
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                using (Intent intent = new Intent(this, typeof(SettingsActivity)))
                {
                    StartActivity(intent);
                }              
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }


        protected void BindViews()
        {
            using (toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.mainToolbar))
            {
                SetSupportActionBar(toolbar);
            }
        }

        private void StartVariables()
        {
            ////CI
            ThreadPool.QueueUserWorkItem(m =>
            {
                    AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa",
                   typeof(Analytics), typeof(Crashes));
                AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa", typeof(Analytics), typeof(Crashes));
            });
            
        }

        //TODO: CHANGE THIS, maybe with a single notification to the user, or not letting him to open the app?
        private void ShowDialog()
        {
            if (NLChecker.IsNotificationListenerEnabled() == false)
            {
                //Prompt a message to go to NotificationListenerService.
                using (Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this))
                {
                    builder.SetMessage(Resource.String.dialognldisabledmessage);
                    builder.SetPositiveButton(Resource.String.dialognldisabledpositivebutton, new EventHandler<DialogClickEventArgs>(OnDialogPositiveButtonEventArgs));
                    builder.SetNegativeButton(Resource.String.dialognldisablednegativebutton, new EventHandler<DialogClickEventArgs>(OnDialogNegativeButtonEventArgs));
                    builder.Show();
                }

            }
        }
        private void OnDialogNegativeButtonEventArgs(object sender, DialogClickEventArgs e)
        {
           Toast.MakeText(this, Resource.String.dialogcancelledclick, ToastLength.Long).Show();
        }
        private void OnDialogPositiveButtonEventArgs(object sender, DialogClickEventArgs e)
        {

            using (Intent intent = new Intent())
            {
                string lel = Android.Provider.Settings.ActionNotificationListenerSettings;
                intent.AddFlags(ActivityFlags.NewTask);
                intent.SetAction(lel);
                StartActivity(intent);
            }
            
        }
    }
}
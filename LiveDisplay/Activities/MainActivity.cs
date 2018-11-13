using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Misc;

//for CI.
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Threading;

namespace LiveDisplay.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/LiveDisplayTheme.NoActionBar", MainLauncher = true)]
    internal class MainActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;
        private TextView status;
        private TextView enableNotificationAccess, enableDeviceAdmin;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            BindViews();
            StartVariables();
        }

        protected override void OnResume()
        {
            CheckNotificationAccess();
            CheckDeviceAdminAccess();
            base.OnResume();
        }

        private void CheckDeviceAdminAccess()
        {
            if (AdminReceiver.isAdminGiven == false)
            {
            }
        }

        private void CheckNotificationAccess()
        {
            if (NLChecker.IsNotificationListenerEnabled() == false)
            {
            }
        }

        private bool IsApplicationHealthy()
        {
            if (NLChecker.IsNotificationListenerEnabled() == true && AdminReceiver.isAdminGiven == true)
            {
                return true;
            }

            return false;
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            enableNotificationAccess.Click -= EnableNotificationAccess_Click;
            enableDeviceAdmin.Click -= EnableDeviceAdmin_Click;
            enableNotificationAccess.Dispose();
            enableDeviceAdmin.Dispose();
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

            status = FindViewById<TextView>(Resource.Id.health);
            enableDeviceAdmin = FindViewById<TextView>(Resource.Id.enableDeviceAccess);
            enableNotificationAccess = FindViewById<TextView>(Resource.Id.enableNotificationAccess);
            enableNotificationAccess.Click += EnableNotificationAccess_Click;
            enableDeviceAdmin.Click += EnableDeviceAdmin_Click;
        }

        private void EnableDeviceAdmin_Click(object sender, EventArgs e)
        {
            ComponentName admin = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));
            using (Intent intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin).PutExtra(DevicePolicyManager.ExtraDeviceAdmin, admin))
                StartActivity(intent);
        }

        private void EnableNotificationAccess_Click(object sender, EventArgs e)
        {
            using (Intent intent = new Intent())
            {
                string lel = Android.Provider.Settings.ActionNotificationListenerSettings;

                intent.AddFlags(ActivityFlags.NewTask);
                intent.SetAction(lel);
                StartActivity(intent);
            }
        }

        private void StartVariables()
        {
            ////CI
            ThreadPool.QueueUserWorkItem(m =>
            {
                AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa", typeof(Analytics), typeof(Crashes));
            });
        }
    }
}
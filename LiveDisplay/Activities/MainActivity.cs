using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.FloatingNotification;

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
        private TextView enableNotificationAccess, enableDeviceAdmin;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            BindViews();
            StartAppCenterMonotoring();
        }

        protected override void OnResume()
        {
            CheckNotificationAccess();
            CheckDeviceAdminAccess();
            IsApplicationHealthy();
            base.OnResume();
        }

        private void CheckDeviceAdminAccess()
        {
            using (var adminGivenImageView = FindViewById<ImageView>(Resource.Id.deviceAccessCheckbox))
            {
                switch (AdminReceiver.isAdminGiven)
                {
                    case true:
                        adminGivenImageView.SetBackgroundResource(Resource.Drawable.check_black_24);
                        break;
                    case false:
                        adminGivenImageView.SetBackgroundResource(Resource.Drawable.denied_black_24);

                        break;
                    default:
                        break;
                }
            }
        }

        private void CheckNotificationAccess()
        {
            using (var notificationAccessGivenImageView = FindViewById<ImageView>(Resource.Id.notificationAccessCheckbox))
            {
                switch (NLChecker.IsNotificationListenerEnabled())
                {
                    case true:
                        notificationAccessGivenImageView.SetBackgroundResource(Resource.Drawable.check_black_24);

                        break;
                    case false:
                        notificationAccessGivenImageView.SetBackgroundResource(Resource.Drawable.denied_black_24);
                        break;
                    default:
                        break;
                }
            }
        }

        private void IsApplicationHealthy()
        {
            using (var accessestext = FindViewById<TextView>(Resource.Id.health))
            {
                if (NLChecker.IsNotificationListenerEnabled() == true && AdminReceiver.isAdminGiven == true)
                {

                    accessestext.SetText(Resource.String.accessesstatusenabled);
                    accessestext.SetTextColor(Android.Graphics.Color.Green);
                }
                else
                {
                    accessestext.SetText(Resource.String.accessesstatusdisabled);
                    accessestext.SetTextColor(Android.Graphics.Color.Red);
                }
            }

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

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 25)
            {
                //Check if the permission to add floating views is allowed
            }
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
                //using (var intent = new Intent(Android.Provider.Settings.ActionManageOverlayPermission))
                //{
                //    StartActivityForResult(intent, 25);
                //}
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

        private void StartAppCenterMonotoring()
        {
            ThreadPool.QueueUserWorkItem(m =>
            {
                AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa", typeof(Analytics), typeof(Crashes));
            });
        }
    }
}
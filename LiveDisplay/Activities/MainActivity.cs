using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Provider;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
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
    [Activity(Label = "@string/app_name", Theme = "@style/LiveDisplayThemeDark.NoActionBar", TaskAffinity = "livedisplay.main", MainLauncher = true)]
    internal class MainActivity : AppCompatActivity
    {
        private static ISharedPreferences configurationManager = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

        private Android.Support.V7.Widget.Toolbar toolbar;
        private TextView enableNotificationAccess, enableDeviceAdmin;
        private TextView enableDrawOverAccess;
        private RelativeLayout enableDrawOverAccessContainer;
        private bool isApplicationHealthy;

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
            if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1)
            {
                CheckDrawOverOtherAppsAccess();
            }
            IsApplicationHealthy();
            base.OnResume();
        }

        private void CheckDeviceAdminAccess()
        {
            using (var adminGivenImageView = FindViewById<ImageView>(Resource.Id.deviceAccessCheckbox))
            {
                switch (Checkers.IsThisAppADeviceAdministrator())
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
                switch (Checkers.IsNotificationListenerEnabled())
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

        private void CheckDrawOverOtherAppsAccess()
        {
            ConfigurationManager configurationManager = new ConfigurationManager(PreferenceManager.GetDefaultSharedPreferences(Application.Context));

            using (var drawOverOtherAppsImageView = FindViewById<ImageView>(Resource.Id.drawOverOtherAppsAccessCheckbox))
            {
                if (Settings.CanDrawOverlays(Application.Context))
                {
                    drawOverOtherAppsImageView.SetBackgroundResource(Resource.Drawable.check_black_24);
                }
                else
                {
                    drawOverOtherAppsImageView.SetBackgroundResource(Resource.Drawable.denied_black_24);
                }
            }
        }

        private void IsApplicationHealthy()
        {
            bool canDrawOverlays = true;
            if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1) //In Lollipop and less this permission is granted at Install time.
            {
                canDrawOverlays = Checkers.ThisAppCanDrawOverlays();
            }

            using (var accessestext = FindViewById<TextView>(Resource.Id.health))
            {
                if (Checkers.IsNotificationListenerEnabled() == true && Checkers.IsThisAppADeviceAdministrator() && canDrawOverlays == true)
                {
                    accessestext.SetText(Resource.String.accessesstatusenabled);
                    accessestext.SetTextColor(Android.Graphics.Color.Green);
                    isApplicationHealthy = true;
                }
                else
                {
                    accessestext.SetText(Resource.String.accessesstatusdisabled);
                    accessestext.SetTextColor(Android.Graphics.Color.Red);
                    isApplicationHealthy = false;
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
                CheckDrawOverOtherAppsAccess();
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
            switch (id)
            {
                case Resource.Id.action_settings:
                    using (Intent intent = new Intent(this, typeof(SettingsActivity)))
                    {
                        intent.AddFlags(ActivityFlags.NewDocument);
                        StartActivity(intent);
                    }

                    return true;

                case Resource.Id.action_sendtestnotification:

                    if (isApplicationHealthy)
                    {
                        using (NotificationSlave slave = NotificationSlave.NotificationSlaveInstance())
                        {
                            var notificationtext = Resources.GetString(Resource.String.testnotificationtext);
                            if (Build.VERSION.SdkInt > BuildVersionCodes.NMr1)
                            {
                                slave.PostNotification("LiveDisplay", notificationtext, true, NotificationImportance.Low);
                            }
                            else
                            {
                                slave.PostNotification("LiveDisplay", notificationtext, true, NotificationPriority.Low);
                            }
                        }
                        using (Intent intent = new Intent(Application.Context, typeof(LockScreenActivity)))
                        {
                            StartActivity(intent);
                            return true;
                        }
                    }
                    else
                    {
                        Toast.MakeText(Application.Context, "You dont have the required permissions yet", ToastLength.Long).Show();
                    }
                    break;

                case Resource.Id.action_help:
                    using (Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this))
                    {
                        builder.SetMessage(Resource.String.helptext);
                        builder.SetPositiveButton("ok, cool", null as EventHandler<DialogClickEventArgs>);
                        builder.Show();
                    }

                    break;

                default:
                    break;
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
            if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1)
            {
                enableDrawOverAccessContainer = FindViewById<RelativeLayout>(Resource.Id.drawOverlaysCheckboxContainer);
                enableDrawOverAccessContainer.Visibility = ViewStates.Visible;
                enableDrawOverAccess = FindViewById<TextView>(Resource.Id.enableFloatingPermission);
                enableDrawOverAccess.Click += EnableDrawOverAccess_Click;
            }
            enableNotificationAccess.Click += EnableNotificationAccess_Click;
            enableDeviceAdmin.Click += EnableDeviceAdmin_Click;
        }

        private void EnableDrawOverAccess_Click(object sender, EventArgs e)
        {
            using (var intent = new Intent(Settings.ActionManageOverlayPermission))
            {
                StartActivityForResult(intent, 25);
            }
        }

        private void EnableDeviceAdmin_Click(object sender, EventArgs e)
        {
            using (Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this))
            {
                builder.SetMessage(Resource.String.dialogfordeviceaccessdescription);
                builder.SetPositiveButton(Resource.String.dialogallowbutton, new EventHandler<DialogClickEventArgs>(OnDialogPositiveButtonEventArgs));
                builder.SetNegativeButton(Resource.String.dialogcancelbutton, new EventHandler<DialogClickEventArgs>(OnDialogNegativeButtonEventArgs));
                builder.Show();
            }
        }

        private void OnDialogNegativeButtonEventArgs(object sender, DialogClickEventArgs e)
        {
        }

        private void OnDialogPositiveButtonEventArgs(object sender, DialogClickEventArgs e)
        {
            ComponentName admin = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));
            using (Intent intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin).PutExtra(DevicePolicyManager.ExtraDeviceAdmin, admin))
                StartActivity(intent);
        }

        private void EnableNotificationAccess_Click(object sender, EventArgs e)
        {
            using (Intent intent = new Intent())
            {
                string lel = Settings.ActionNotificationListenerSettings;

                intent.AddFlags(ActivityFlags.NewTask);
                intent.SetAction(lel);
                StartActivity(intent);
            }
        }

        private void StartAppCenterMonotoring()
        {
            ThreadPool.QueueUserWorkItem(m =>
            {
                AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa", typeof(Analytics), typeof(Crashes), typeof(ErrorReport));
            });
        }
    }
}
namespace LiveDisplay.Activities
{
    using Android.App;
    using Android.App.Admin;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using Android.Provider;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using AndroidX.AppCompat.App;
    using AndroidX.Preference;
    using LiveDisplay.BroadcastReceivers;
    using LiveDisplay.Misc;
    using LiveDisplay.Services;
    using LiveDisplay.Services.Awake;
    using LiveDisplay.Services.Wallpaper;

    //for CI.
    using Microsoft.AppCenter;
    using Microsoft.AppCenter.Analytics;
    using Microsoft.AppCenter.Crashes;
    using System;
    using System.Threading;
    using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
    using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

    [Activity(Label = "@string/app_name", Theme = "@style/LiveDisplayThemeDark.NoActionBar", TaskAffinity = "livedisplay.main", MainLauncher = true)]
    internal class MainActivity : AppCompatActivity
    {
        private readonly int REQUEST_CODE_READ_STORAGE_PERMISSION = 1;

        private Toolbar toolbar;
        private TextView enableNotificationAccess, enableDeviceAdmin;
        private TextView enableStoragePermission;
        private RelativeLayout enableStorageAccessContainer;
        private bool isApplicationHealthy;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            WallpaperPublisher.NewWallpaperIssued += WallpaperPublisher_NewWallpaperIssued;
            BindViews();
            StartAppCenterMonotoring();
        }

        protected override void OnResume()
        {
            CheckNotificationAccess();
            CheckDeviceAdminAccess();
            CheckDrawOverOtherAppsAccess();
            CheckStorageAccess();
            IsApplicationHealthy();
            LoadDefaultSettings();
            base.OnResume();
        }

        private void LoadDefaultSettings()
        {
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.awake_prefs, true);
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.lockscreen_prefs, true);
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.music_widget_prefs, true);
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.notification_prefs, true);
            PreferenceManager.SetDefaultValues(Application.Context, "weatherpreferences", (int)FileCreationMode.Private, Resource.Xml.weather_widget_prefs, true);
        }

        private void WallpaperPublisher_NewWallpaperIssued(object sender, WallpaperChangedEventArgs e)
        {
            if (e.Wallpaper?.Bitmap != null)
            {
                RunOnUiThread(() =>
                {
                    Window.DecorView.Background = e.Wallpaper;
                });
            }
        }

        private void AdminReceiver_OnDeviceAdminEnabled(object sender, bool e)
        {
            using (var adminGivenImageView = FindViewById<ImageView>(Resource.Id.deviceAccessCheckbox))
            {
                RunOnUiThread(() =>
                {
                    switch (e)
                    {
                        case true:
                            adminGivenImageView.SetBackgroundResource(Resource.Drawable.check_black_24);
                            break;

                        case false:
                            adminGivenImageView.SetBackgroundResource(Resource.Drawable.denied_black_24);
                            break;
                    }
                });
                ThreadPool.QueueUserWorkItem(m =>
                {
                    Thread.Sleep(500);
                    IsApplicationHealthy();
                });
            }
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
                }
            }
        }

        private void CheckDrawOverOtherAppsAccess()
        {
            using (var drawOverOtherAppsImageView = FindViewById<ImageView>(Resource.Id.drawOverOtherAppsAccessCheckbox))
            {
                if (Checkers.ThisAppCanDrawOverlays())
                {
                    drawOverOtherAppsImageView.SetBackgroundResource(Resource.Drawable.check_black_24);
                }
                else
                {
                    drawOverOtherAppsImageView.SetBackgroundResource(Resource.Drawable.denied_black_24);
                }
            }
        }
        private void CheckStorageAccess()
        {
            using (var readStorageAccessImageView = FindViewById<ImageView>(Resource.Id.readStorageAccessCheckbox))
            {
                if (Checkers.ThisAppHasReadStoragePermission())
                {
                    readStorageAccessImageView.SetBackgroundResource(Resource.Drawable.check_black_24);
                }
                else
                {
                    readStorageAccessImageView.SetBackgroundResource(Resource.Drawable.denied_black_24);
                }
            }
        }

        private void IsApplicationHealthy()
        {
            using (var accessestext = FindViewById<TextView>(Resource.Id.health))
            {
                RunOnUiThread(() =>
                {
                    if (Checkers.IsNotificationListenerEnabled() && Checkers.ThisAppHasReadStoragePermission())
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
                });
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            AdminReceiver.OnDeviceAdminEnabled -= AdminReceiver_OnDeviceAdminEnabled;
        }

        protected override void OnDestroy()
        {
            enableNotificationAccess.Click -= EnableNotificationAccess_Click;
            enableDeviceAdmin.Click -= EnableDeviceAdmin_Click;
            WallpaperPublisher.NewWallpaperIssued -= WallpaperPublisher_NewWallpaperIssued;
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
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            using (var readStorageAccessImageView = FindViewById<ImageView>(Resource.Id.readStorageAccessCheckbox))
            {
                if (requestCode == REQUEST_CODE_READ_STORAGE_PERMISSION && grantResults[0] == Permission.Granted)
                {
                    readStorageAccessImageView.SetBackgroundResource(Resource.Drawable.check_black_24);
                }
                else
                {
                    readStorageAccessImageView.SetBackgroundResource(Resource.Drawable.denied_black_24);
                }
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
                        AwakeHelper.TurnOffScreen();
                        using (NotificationSlave slave = NotificationSlave.NotificationSlaveInstance())
                        {
                            var notificationtext = Resources.GetString(Resource.String.testnotificationtext);
                            if (Build.VERSION.SdkInt > BuildVersionCodes.NMr1)
                            {
                                slave.PostNotification(7, "LiveDisplay", notificationtext, true, NotificationImportance.Max);
                            }
                            else
                            {
                                slave.PostNotification(1, "LiveDisplay", notificationtext, true, NotificationPriority.Max);
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
                    using (AlertDialog.Builder builder = new AlertDialog.Builder(this))
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
            using (toolbar = FindViewById<Toolbar>(Resource.Id.mainToolbar))
            {
                SetSupportActionBar(toolbar);
            }

            enableDeviceAdmin = FindViewById<TextView>(Resource.Id.enableDeviceAccess);
            enableNotificationAccess = FindViewById<TextView>(Resource.Id.enableNotificationAccess);
            //if (Build.VERSION.SdkInt > BuildVersionCodes.LollipopMr1)
            //{
            //    enableDrawOverAccessContainer = FindViewById<RelativeLayout>(Resource.Id.drawOverlaysCheckboxContainer);
            //    enableDrawOverAccessContainer.Visibility = ViewStates.Visible;
            //    enableDrawOverAccess = FindViewById<TextView>(Resource.Id.enableFloatingPermission);
            //    enableDrawOverAccess.Click += EnableDrawOverAccess_Click;
            //}            //You won't be needing the permission for now.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                enableStorageAccessContainer = FindViewById<RelativeLayout>(Resource.Id.readPhoneStorageContainer);
                enableStorageAccessContainer.Visibility = ViewStates.Visible;
                enableStoragePermission = FindViewById<TextView>(Resource.Id.enableStoragePermission);
                enableStoragePermission.Click += EnableStoragePermission_Click;
            }


            enableNotificationAccess.Click += EnableNotificationAccess_Click;
            enableDeviceAdmin.Click += EnableDeviceAdmin_Click;
            
        }

        private void EnableDrawOverAccess_Click(object sender, EventArgs e)
        {
            using (var intent = new Intent(Settings.ActionManageOverlayPermission))
                StartActivityForResult(intent, 25);
        }
        private void EnableStoragePermission_Click(object sender, EventArgs e)
        {
            RequestPermissions(new string[1] { "android.permission.READ_EXTERNAL_STORAGE" }, REQUEST_CODE_READ_STORAGE_PERMISSION);
        }

        private void EnableDeviceAdmin_Click(object sender, EventArgs e)
        {
            if (Checkers.IsThisAppADeviceAdministrator())
            {
                ComponentName devAdminReceiver = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));
                DevicePolicyManager dpm = (DevicePolicyManager)GetSystemService(Context.DevicePolicyService);
                dpm.RemoveActiveAdmin(devAdminReceiver);
                CheckDeviceAdminAccess();
            }
            else
            {
                using (AlertDialog.Builder builder = new AlertDialog.Builder(this))
                {
                    builder.SetMessage(Resource.String.dialogfordeviceaccessdescription);
                    builder.SetPositiveButton(Resource.String.dialogallowbutton, new EventHandler<DialogClickEventArgs>(OnDialogPositiveButtonEventArgs));
                    builder.SetNegativeButton(Resource.String.dialogcancelbutton, null as EventHandler<DialogClickEventArgs>);
                    builder.Show();
                }
            }
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
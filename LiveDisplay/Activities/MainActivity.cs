namespace LiveDisplay.Activities
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using AndroidX.AppCompat.App;
    using AndroidX.AppCompat.Widget;
    using LiveDisplay.Misc;

    //for CI.
    using Microsoft.AppCenter;
    using System;
    using System.Threading;
    using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
    using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

    [Activity(Label = "@string/app_name", MainLauncher = true)]
    internal class MainActivity : AppCompatActivity
    {
        private Toolbar toolbar;
        private RelativeLayout enableNotificationAccess, enableDeviceAdmin, enablePostingNotifications, 
            enableAccessibilityAccess, enableLocationAccess, enableShowOnLockScreen, enableRecordAudioAccess;
        public static int StartCount = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetContentView(Resource.Layout.Main);
            BindViews();
            StartAppCenterMonitoring();
            base.OnCreate(savedInstanceState);
        }

        protected override void OnResume()
        {
            CheckAllPermissions();
            IsApplicationHealthy();
            base.OnResume();
        }

        private void CheckAllPermissions(){

            SetPermissionStatus(Checkers.ThisAppCanPostNotifications(), Resource.Id.post_notifications_permission_checkbox);
            SetPermissionStatus(Checkers.IsNotificationListenerEnabled(), Resource.Id.read_notifications_permission_checkbox);
            SetPermissionStatus(Checkers.IsAccessibilityEnabled(), Resource.Id.accessibility_access_permission_checkbox);
            SetPermissionStatus(Checkers.IsThisAppADeviceAdministrator(), Resource.Id.device_access_permission_checkbox);
            SetPermissionStatus(Checkers.ThisAppCanReadLocation(), Resource.Id.location_access_permission_checkbox);
            SetPermissionStatus(Checkers.ThisAppCanBeShownOnXiaomiDeviceLockScreen(), Resource.Id.show_on_lock_screen_xiaomi_permission_checkbox, true);
            SetPermissionStatus(Checkers.ThisAppCanRecordAudio(), Resource.Id.record_audio_access_checkbox);
        }
        private void SetPermissionStatus(bool isPermissionAllowed, int resourceRepresentingPermissionStatus, bool unknownStatusPermission = false)
        {
            using var permissionImageView = FindViewById<AppCompatImageView>(resourceRepresentingPermissionStatus);
            switch (isPermissionAllowed)
            {
                case true:
                    permissionImageView.SetBackgroundResource(Resource.Drawable.outline_check_white_24);
                    break;
                case false:
                    permissionImageView.SetBackgroundResource(Resource.Drawable.outline_close_white_24);
                    break;
            }
            if(unknownStatusPermission)
            {
                permissionImageView.SetBackgroundResource(Resource.Drawable.ic_warning_white_24dp);
            }
        }


        private void IsApplicationHealthy()
        {
            using (var accessestext = FindViewById<TextView>(Resource.Id.health))
            {
                if (Checkers.AreMandatoryPermissionsEnabled())
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
        protected override void OnDestroy()
        {
            enableNotificationAccess.Click -= EnableNotificationAccess_Click;
            enableDeviceAdmin.Click -= EnableDeviceAdmin_Click;
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
            switch (id)
            {
                case Resource.Id.action_settings:
                    using (Intent intent = new Intent(this, typeof(SettingsActivity)))
                    {
                        StartActivity(intent);
                    }

                    return true;

                case Resource.Id.show_lock_screen_preview:

                    if (Checkers.AreMandatoryPermissionsEnabled())
                    {
                        using (Intent intent = new Intent(Application.Context, typeof(LockScreenActivity)))
                        {
                            StartActivity(intent);
                            return true;
                        }
                    }
                    else
                    {
                        Toast.MakeText(Application.Context, GetString(Resource.String.not_enough_permissions), ToastLength.Long).Show();
                    }
                    break;

                case Resource.Id.action_help:
                    using (AlertDialog.Builder builder = new AlertDialog.Builder(this))
                    {
                        builder.SetMessage(Resource.String.helptext);
                        builder.SetPositiveButton(Resource.String.ok, null as EventHandler<DialogClickEventArgs>);
                        builder.Show();
                    }

                    break;

                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            var result = (data!= null && data.Extras!=null) && data.Extras.GetBoolean(Permissions.PermissionKey, false);
            switch(requestCode)
            {
                case Permissions.PostNotifications:
                    SetPermissionStatus(result, Resource.Id.post_notifications_permission_checkbox);
                    break;
                case Permissions.ReadNotifications:
                    SetPermissionStatus(result, Resource.Id.read_notifications_permission_checkbox);
                    break;
                case Permissions.DeviceAdmin:
                    SetPermissionStatus(result, Resource.Id.device_access_permission_checkbox);
                    break;
                case Permissions.EnableAccessibilityService:
                    SetPermissionStatus(result, Resource.Id.accessibility_access_permission_checkbox);
                    break;
                case Permissions.Location:
                    SetPermissionStatus(result, Resource.Id.location_access_permission_checkbox);
                    break;
                case Permissions.ShowOnLockScreenXiaomi:
                    SetPermissionStatus(result, Resource.Id.show_on_lock_screen_xiaomi_permission_checkbox, true);
                    break;
                case Permissions.RecordAudio:
                    SetPermissionStatus(result, Resource.Id.show_on_lock_screen_xiaomi_permission_checkbox);
                    break;
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }
        protected void BindViews()
        {
            using (toolbar = FindViewById<Toolbar>(Resource.Id.mainToolbar))
            {
                SetSupportActionBar(toolbar);
            }

            enableDeviceAdmin = FindViewById<RelativeLayout>(Resource.Id.device_access_permission);
            enableAccessibilityAccess = FindViewById<RelativeLayout>(Resource.Id.accessibility_access_permission);
            enableNotificationAccess = FindViewById<RelativeLayout>(Resource.Id.read_notifications_permission);
            enableLocationAccess = FindViewById<RelativeLayout>(Resource.Id.location_access_permission);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                enablePostingNotifications = FindViewById<RelativeLayout>(Resource.Id.post_notifications_permission);
                enablePostingNotifications.Visibility = ViewStates.Visible;
                enablePostingNotifications.Click += EnablePostingNotifications_Click;
            }

            enableNotificationAccess.Click += EnableNotificationAccess_Click;
            enableDeviceAdmin.Click += EnableDeviceAdmin_Click;
            enableAccessibilityAccess.Click += EnableAccessibilityAccess_Click;
            enableLocationAccess.Click += EnableLocationAccess_Click;
            enableRecordAudioAccess = FindViewById<RelativeLayout>(Resource.Id.record_audio_access);
            enableRecordAudioAccess.Click += EnableRecordAudioAccess_Click;

        }

        private void EnableRecordAudioAccess_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, Java.Lang.Class.FromType(typeof(PermissionExplanationActivity)));
            var extras = new Bundle();
            extras.PutInt(Permissions.PermissionKey, Permissions.RecordAudio);
            intent.PutExtras(extras);

            StartActivityForResult(intent, Permissions.RecordAudio);
        }

        private void EnableShowOnLockScreen_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, Java.Lang.Class.FromType(typeof(PermissionExplanationActivity)));
            var extras = new Bundle();
            extras.PutInt(Permissions.PermissionKey, Permissions.ShowOnLockScreenXiaomi);
            intent.PutExtras(extras);

            StartActivityForResult(intent, Permissions.ShowOnLockScreenXiaomi);
        }

        private void EnableLocationAccess_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, Java.Lang.Class.FromType(typeof(PermissionExplanationActivity)));
            var extras = new Bundle();
            extras.PutInt(Permissions.PermissionKey, Permissions.Location);
            intent.PutExtras(extras);

            StartActivityForResult(intent, Permissions.Location);
        }

        private void EnablePostingNotifications_Click(object sender, EventArgs e)
        {

            var intent = new Intent(this, Java.Lang.Class.FromType(typeof(PermissionExplanationActivity)));
            var extras = new Bundle();
            extras.PutInt(Permissions.PermissionKey, Permissions.PostNotifications);
            intent.PutExtras(extras);

            StartActivityForResult(intent, Permissions.PostNotifications);

        }

        private void EnableDrawOverAccess_Click(object sender, EventArgs e)
        {
            //activityResultLauncher.Launch(Settings.ActionManageOverlayPermission);
        }

        private void EnableDeviceAdmin_Click(object sender, EventArgs e)
        {

            var intent = new Intent(this, Java.Lang.Class.FromType(typeof(PermissionExplanationActivity)));
            var extras = new Bundle();
            extras.PutInt(Permissions.PermissionKey, Permissions.DeviceAdmin);
            intent.PutExtras(extras);

            StartActivityForResult(intent, Permissions.DeviceAdmin);

        }

        private void EnableAccessibilityAccess_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, Java.Lang.Class.FromType(typeof(PermissionExplanationActivity)));
            var extras = new Bundle();
            extras.PutInt(Permissions.PermissionKey, Permissions.EnableAccessibilityService);
            intent.PutExtras(extras);

            StartActivityForResult(intent, Permissions.EnableAccessibilityService);
        }

        private void EnableNotificationAccess_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, Java.Lang.Class.FromType(typeof(PermissionExplanationActivity)));
            var extras = new Bundle();
            extras.PutInt(Permissions.PermissionKey, Permissions.ReadNotifications);
            intent.PutExtras(extras);

            StartActivityForResult(intent, Permissions.ReadNotifications);
        }

        private void StartAppCenterMonitoring()
        {
            ThreadPool.QueueUserWorkItem(m =>
            {
#if DEBUG
                Console.WriteLine("Start Appcenter here");
#else
                Microsoft.AppCenter.AppCenter.Start("0ec5320c-34b4-498b-a9c2-dae7614997fa", typeof(Microsoft.AppCenter.Analytics.Analytics), 
                typeof(Microsoft.AppCenter.Crashes.Crashes), typeof(Microsoft.AppCenter.Crashes.ErrorReport));
#endif
            });
        }
    }
}
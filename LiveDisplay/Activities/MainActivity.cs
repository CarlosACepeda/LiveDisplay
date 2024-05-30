namespace LiveDisplay.Activities
{
    using Android.App;
    using Android.App.Admin;
    using Android.Content;
    using Android.Net;
    using Android.OS;
    using Android.Provider;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using AndroidX.Activity.Result;
    using AndroidX.AppCompat.App;
    using AndroidX.AppCompat.Widget;
    using LiveDisplay.BroadcastReceivers;
    using LiveDisplay.Misc;
    using LiveDisplay.Servicios;
    using LiveDisplay.Servicios.Awake;

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
        private RelativeLayout enableNotificationAccess, enableDeviceAdmin, enablePostingNotifications, enableAccessibilityAccess;
        private bool isApplicationHealthy;
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
        }

        private void SetPermissionStatus(bool isPermissionAllowed, int resourceRepresentingPermissionStatus)
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
        }


        private void IsApplicationHealthy()
        {
            using (var accessestext = FindViewById<TextView>(Resource.Id.health))
            {
                if (Checkers.IsNotificationListenerEnabled() && 
                    Checkers.ThisAppCanPostNotifications())
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

                case Resource.Id.action_sendtestnotification:

                    if (isApplicationHealthy)
                    {
                        AwakeHelper.TurnOffScreen();
                        using (NotificationSlave slave = NotificationSlave.GetInstance())
                        {
                            var notificationtext = GetString(Resource.String.testnotificationtext);
                            if (Build.VERSION.SdkInt > BuildVersionCodes.NMr1)
                            {
                                slave.PostNotification(1, "LiveDisplay", notificationtext, true, NotificationImportance.Max);
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
            enablePostingNotifications = FindViewById<RelativeLayout>(Resource.Id.post_notifications_permission);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                enablePostingNotifications.Visibility = ViewStates.Visible;
                enablePostingNotifications.Click += EnablePostingNotifications_Click;
            }

            enableNotificationAccess.Click += EnableNotificationAccess_Click;
            enableDeviceAdmin.Click += EnableDeviceAdmin_Click;
            enableAccessibilityAccess.Click += EnableAccessibilityAccess_Click;

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



            //if (Checkers.IsThisAppADeviceAdministrator())
            //{
            //    ComponentName devAdminReceiver = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));
            //    DevicePolicyManager dpm = (DevicePolicyManager)GetSystemService(DevicePolicyService);
            //    dpm.RemoveActiveAdmin(devAdminReceiver);
            //}
            //else
            //{
            //    using (AlertDialog.Builder builder = new AlertDialog.Builder(this))
            //    {
            //        builder.SetMessage(Resource.String.dialogfordeviceaccessdescription);
            //        builder.SetPositiveButton(Resource.String.dialogallowbutton, new EventHandler<DialogClickEventArgs>(OnDialogPositiveButtonEventArgs));
            //        builder.SetNegativeButton(Resource.String.dialogcancelbutton, null as EventHandler<DialogClickEventArgs>);
            //        builder.Show();
            //    }
            //}
        }

        private void EnableAccessibilityAccess_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, Java.Lang.Class.FromType(typeof(PermissionExplanationActivity)));
            var extras = new Bundle();
            extras.PutInt(Permissions.PermissionKey, Permissions.EnableAccessibilityService);
            intent.PutExtras(extras);

            StartActivityForResult(intent, Permissions.EnableAccessibilityService);
        }


        private void OnDialogPositiveButtonEventArgs(object sender, DialogClickEventArgs e)
        {
            //ComponentName admin = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));
            //using Intent intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin).PutExtra(DevicePolicyManager.ExtraDeviceAdmin, admin);
            //StartActivity(intent);
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
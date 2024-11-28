using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Widget;
using AndroidX.Activity.Result;
using AndroidX.AppCompat.App;
using Java.Lang;
using LiveDisplay;
using LiveDisplay.BroadcastReceivers;
using LiveDisplay.Misc;
using LiveDisplay.Services;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;

[Activity(Label = "@string/permission_explanation_activity_label")]
public class PermissionExplanationActivity: AppCompatActivity, IActivityResultCallback
{
    int _permissionToSetRequestCode = Permissions.None;
    int permissionBeingSetForResult = Permissions.None;
    ActivityResultLauncher activityResultLauncher;


    Button accept_permission, deny_permission;
    TextView permission_title, permission_explanation;

    bool permissionAlreadyGranted = false;

    const string XiaomiSecurityCenterPackage = "com.miui.securitycenter";
    const string XiaomiSecurityCenterActivityToStart ="com.miui.permcenter.permissions.PermissionsEditorActivity";
    const string XiaomiSecurityCenterIntentAction = "miui.intent.action.APP_PERM_EDITOR";
    const string XiaomiSecurityCenterIntentExtraTargetPackageKey = "extra_pkgname";
    protected override void OnCreate(Bundle savedInstanceState)
    {
        SetContentView(Resource.Layout.permission_explanation);

        activityResultLauncher = RegisterForActivityResult(new RequestPermission(), this);


        accept_permission = FindViewById<Button>(Resource.Id.accept_permission);
        deny_permission = FindViewById<Button>(Resource.Id.deny_permission);
        permission_title = FindViewById<TextView>(Resource.Id.permission_title);
        permission_explanation = FindViewById<TextView>(Resource.Id.permission_explanation);

        accept_permission.Click += Accept_permission_Click;
        deny_permission.Click += Deny_permission_Click;

        _permissionToSetRequestCode = Intent.Extras.GetInt(Permissions.PermissionKey);

        SetExplanationAndStatus();

        base.OnCreate(savedInstanceState);
    }

    private void Deny_permission_Click(object sender, System.EventArgs e)
    {
        if (permissionAlreadyGranted)
            SetPermissionResult(true);
        else
            SetPermissionResult(false);
    }

    private void Accept_permission_Click(object sender, System.EventArgs e)
    {
        var intent = new Intent();

        switch (_permissionToSetRequestCode)
        {
            case Permissions.PostNotifications:
                activityResultLauncher.Launch(Android.Manifest.Permission.PostNotifications); //Asking for a runtime permission
                permissionBeingSetForResult = _permissionToSetRequestCode;
                break;
            case Permissions.Location:
                activityResultLauncher.Launch(Android.Manifest.Permission.AccessCoarseLocation); //Asking for a runtime permission
                permissionBeingSetForResult = _permissionToSetRequestCode;
                break;
            case Permissions.ReadNotifications:
                ComponentName readNotifications = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(Catcher)));
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    intent = new Intent(Settings.ActionNotificationListenerSettings);
                }
                else
                {
                    intent = new Intent(Settings.ActionNotificationListenerDetailSettings);
                    intent.PutExtra(Settings.ExtraNotificationListenerComponentName, readNotifications.FlattenToString());
                }
                break;
            case Permissions.EnableAccessibilityService:
                intent.SetAction(Settings.ActionAccessibilitySettings);
                break;
            case Permissions.DeviceAdmin:

                ComponentName admin = new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(AdminReceiver)));
                intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin).PutExtra(DevicePolicyManager.ExtraDeviceAdmin, admin);
                break;
            case Permissions.ShowOnLockScreenXiaomi:
                    intent = new Intent(XiaomiSecurityCenterIntentAction);
                    intent.SetClassName(XiaomiSecurityCenterPackage,XiaomiSecurityCenterActivityToStart);
                    intent.PutExtra(XiaomiSecurityCenterIntentExtraTargetPackageKey, this.PackageName);
                break;
            case Permissions.RecordAudio:
                activityResultLauncher.Launch(Android.Manifest.Permission.RecordAudio); //Asking for a runtime permission
                permissionBeingSetForResult = _permissionToSetRequestCode;
                break;
        }
        //To prevent launching activity twice, as asking for Runtime permissions is made by Activity Result Launcher
        if (permissionBeingSetForResult == Permissions.None)
            StartActivityForResult(intent, _permissionToSetRequestCode);
    }
    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        //Only for activities started, not for requested Manifest Runtime Permissions, such as posting notifications.
        switch (requestCode)
        {
            case Permissions.ReadNotifications:
                SetPermissionResult(Checkers.IsNotificationListenerEnabled());
                break;
            case Permissions.DeviceAdmin:
                SetPermissionResult(Checkers.IsThisAppADeviceAdministrator());
                break;
            case Permissions.EnableAccessibilityService:
                SetPermissionResult(Checkers.IsAccessibilityEnabled());
                break;
            case Permissions.ShowOnLockScreenXiaomi:
                SetPermissionResult(Checkers.ThisAppCanBeShownOnXiaomiDeviceLockScreen());
                break;
        }

        base.OnActivityResult(requestCode, resultCode, data);
    }
    public void OnActivityResult(Object result)
    {
        switch (permissionBeingSetForResult)
        {
            case Permissions.PostNotifications:
                SetPermissionResult(Checkers.ThisAppCanPostNotifications());
                break;
            case Permissions.Location:
                SetPermissionResult(Checkers.ThisAppCanReadLocation());
                break;
            case Permissions.RecordAudio:
                SetPermissionResult(Checkers.ThisAppCanRecordAudio());
                break;
        }
    }

    void SetPermissionResult(bool permissionResult)
    {
        var resultIntent = new Intent();
        var extras = new Bundle();
        extras.PutBoolean(Permissions.PermissionKey, permissionResult);
        permissionBeingSetForResult = Permissions.None;
        resultIntent.PutExtras(extras);
        SetResult(Android.App.Result.Ok, resultIntent);
        Finish();
    }

    public void SetExplanationAndStatus()
    {
        string title= string.Empty;
        string explanation= string.Empty;
        switch(_permissionToSetRequestCode)
        {
            case Permissions.ReadNotifications:
                title = GetString(Resource.String.read_notifications_title);
                explanation = GetString(Resource.String.read_notifications_explanation);
                permissionAlreadyGranted = Checkers.IsNotificationListenerEnabled();
                break;
            case Permissions.PostNotifications:
                title = GetString(Resource.String.post_notifications_title);
                explanation = GetString(Resource.String.post_notifications_explanation);
                permissionAlreadyGranted = Checkers.ThisAppCanPostNotifications();
                break;
            case Permissions.EnableAccessibilityService:
                title = GetString(Resource.String.enable_accessibility_title);
                explanation = GetString(Resource.String.enable_accessibility_explanation);
                permissionAlreadyGranted = Checkers.IsAccessibilityEnabled();
                break;
            case Permissions.DeviceAdmin:
                title = GetString(Resource.String.device_admin_title);
                explanation = GetString(Resource.String.device_admin_explanation);
                permissionAlreadyGranted = Checkers.IsThisAppADeviceAdministrator();
                break;
            case Permissions.Location:
                title = GetString(Resource.String.access_location_title);
                explanation = GetString(Resource.String.access_location_explanation);
                permissionAlreadyGranted = Checkers.ThisAppCanReadLocation();
                break;
            case Permissions.ShowOnLockScreenXiaomi:
                title = GetString(Resource.String.show_on_lock_screen_xiaomi_title);
                explanation = GetString(Resource.String.show_on_lock_screen_xiaomi_explanation);
                permissionAlreadyGranted = Checkers.ThisAppCanBeShownOnXiaomiDeviceLockScreen();
                break;
            case Permissions.RecordAudio:
                title = GetString(Resource.String.record_audio_access_title);
                explanation = GetString(Resource.String.record_audio_access_explanation);
                permissionAlreadyGranted = Checkers.ThisAppCanRecordAudio();
                break;

        }
        permission_title.Text= title;
        permission_explanation.Text= explanation;

        if (permissionAlreadyGranted)
        {
            accept_permission.Enabled = false;
            accept_permission.Text = GetString(Resource.String.you_already_have_permission);
            deny_permission.Text = GetString(Resource.String.close);
        }
    }
}
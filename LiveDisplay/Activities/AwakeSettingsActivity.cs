using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Activities
{
    [Activity(Label = "@string/awakesettings")]
    public class AwakeSettingsActivity : AppCompatActivity
    {
        private CheckBox cbEnableAwake;
        private SwitchCompat swTurnOnNewNotifications;
        private SwitchCompat swTurnOnUserMovement;
        private Android.Support.V7.Widget.Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AwakeSettings);
            BindViews();
            // Create your application here
        }

        private void BindViews()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            }
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.awakeToolbar);
            SetSupportActionBar(toolbar);

            cbEnableAwake = FindViewById<CheckBox>(Resource.Id.cbEnableAwake);
            swTurnOnNewNotifications = FindViewById<SwitchCompat>(Resource.Id.swTurnOnNewNotifications);
            swTurnOnUserMovement = FindViewById<SwitchCompat>(Resource.Id.swTurnOnUserMovement);
        }
    }
}
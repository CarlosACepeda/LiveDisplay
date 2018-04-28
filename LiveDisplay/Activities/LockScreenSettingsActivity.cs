using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Activities
{
    [Activity(Label = "@string/ajusteslockscreen")]
    public class LockScreenSettingsActivity : AppCompatActivity
    {
        private CheckBox cbEnableLockScreen;
        private SwitchCompat swToggleClock;
        private SwitchCompat swToggleSystemIcons;
        private SwitchCompat swUseLockscreenNoNotifications;
        private SwitchCompat swDynamicWallpaper;
        private Android.Support.V7.Widget.Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.LockScreenSettings);
            BindViews();
        }

        private void BindViews()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            }
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.lockscreenSettingsToolbar);
            SetSupportActionBar(toolbar);

            cbEnableLockScreen = FindViewById<CheckBox>(Resource.Id.cbEnableLockScreen);
            swToggleClock = FindViewById<SwitchCompat>(Resource.Id.swToggleClock);
            swToggleSystemIcons = FindViewById<SwitchCompat>(Resource.Id.swToggleSystemIcons);
            swUseLockscreenNoNotifications = FindViewById<SwitchCompat>(Resource.Id.swUseLockscreenNoNotifications);
            swDynamicWallpaper = FindViewById<SwitchCompat>(Resource.Id.swDynamicWallpaper);
        }
    }
}
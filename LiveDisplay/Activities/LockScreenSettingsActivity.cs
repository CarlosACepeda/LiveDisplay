using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using LiveDisplay.Servicios;
using LiveDisplay.Misc;
using System;

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
        private Button btnChangeWallpaper;
        private Android.Support.V7.Widget.Toolbar toolbar;

        private ConfigurationManager configurationManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.LockScreenSettings);
            configurationManager = new ConfigurationManager(GetPreferences(FileCreationMode.Private));
            BindViews();
            BindClickEvents();
            RetrieveLockScreenConfiguration();
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
            btnChangeWallpaper = FindViewById<Button>(Resource.Id.btnChangeWallpaper);
        }
        private void BindClickEvents()
        {
            cbEnableLockScreen.CheckedChange += CbEnableLockScreen_CheckedChange;
            swToggleClock.CheckedChange += SwToggleClock_CheckedChange;
            swToggleSystemIcons.CheckedChange += SwToggleSystemIcons_CheckedChange;
            swUseLockscreenNoNotifications.CheckedChange += SwUseLockscreenNoNotifications_CheckedChange;
            swDynamicWallpaper.CheckedChange += SwDymamicWallpaper_CheckedChange;
            btnChangeWallpaper.Click += BtnChangeWallpaper_Click;
          
        }      
        private void UnbindClickEvents()
        {
            cbEnableLockScreen.CheckedChange -= CbEnableLockScreen_CheckedChange;
            swToggleClock.CheckedChange -= SwToggleClock_CheckedChange;
            swToggleSystemIcons.CheckedChange -= SwToggleSystemIcons_CheckedChange;
            swUseLockscreenNoNotifications.CheckedChange -= SwUseLockscreenNoNotifications_CheckedChange;
            swDynamicWallpaper.CheckedChange -= SwDymamicWallpaper_CheckedChange;
            btnChangeWallpaper.Click -= BtnChangeWallpaper_Click;
        }
        private void CbEnableLockScreen_CheckedChange(object sender, EventArgs e)
        {

            if (cbEnableLockScreen.Checked == true)
            {
                configurationManager.SaveAValue("enabledlockscreen?", true);
            }
            else
            {
                configurationManager.SaveAValue("enabledlockscreen?", false);
            }
            
        }
        private void SwToggleClock_CheckedChange(object sender, EventArgs e)
        {
            if (swToggleClock.Checked == true)
            {
                configurationManager.SaveAValue("hiddenclock?", true);
            }
            else
            {
                configurationManager.SaveAValue("hiddenclock?", false);
            }
        }
        private void SwToggleSystemIcons_CheckedChange(object sender, EventArgs e)
        {
            if (swToggleSystemIcons.Checked == true)
            {
                configurationManager.SaveAValue(ConfigurationParameters.hiddensystemicons, true);
            }
            else
            {
                configurationManager.SaveAValue(ConfigurationParameters.hiddensystemicons, false);
            }
        }
        private void SwUseLockscreenNoNotifications_CheckedChange(object sender, EventArgs e)
        {
            if (swUseLockscreenNoNotifications.Checked == true)
            {
                configurationManager.SaveAValue(ConfigurationParameters.enabledlockscreennonotifications, true);
            }
            else
            {
                configurationManager.SaveAValue(ConfigurationParameters.enabledlockscreennonotifications, false);
            }
        }
        private void SwDymamicWallpaper_CheckedChange(object sender, EventArgs e)
        {
            if (swDynamicWallpaper.Checked == true)
            {
                configurationManager.SaveAValue(ConfigurationParameters.dynamicwallpaperdisabled, true);
            }
            else
            {
                configurationManager.SaveAValue(ConfigurationParameters.dynamicwallpaperdisabled, false);
            }
        }
        private void BtnChangeWallpaper_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        private void RetrieveLockScreenConfiguration()
        {
            cbEnableLockScreen.Checked = configurationManager.RetrieveAValue(ConfigurationParameters.enabledLockscreen) == true ? true : false;
            swToggleClock.Checked = configurationManager.RetrieveAValue(ConfigurationParameters.hiddenclock) == true ? true : false;
            swToggleSystemIcons.Checked = configurationManager.RetrieveAValue(ConfigurationParameters.hiddensystemicons) == true ? true : false;
            swUseLockscreenNoNotifications.Checked = configurationManager.RetrieveAValue(ConfigurationParameters.enabledlockscreennonotifications) == true ? true : false;
            swDynamicWallpaper.Checked = configurationManager.RetrieveAValue(ConfigurationParameters.dynamicwallpaperdisabled) == true ? true : false;

        }
    }
}
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
using Android.Runtime;
using Android.Graphics.Drawables;
using Java.IO;
using Android.Graphics;
using Android.Provider;
using Android.Util;
using LiveDisplay.Factories;
using LiveDisplay.Fragments;

namespace LiveDisplay.Activities
{
    //prepare for deprecation, this will be Settings Screen
    [Activity(Label = "@string/settings")]
    public class SettingsActivity : AppCompatActivity
    {
        private SwitchCompat swToggleClock;
        private SwitchCompat swToggleSystemIcons;
        private SwitchCompat swUseLockscreenNoNotifications;
        private SwitchCompat swDynamicWallpaper;
        private Button btnChangeWallpaper;
        private Android.Support.V7.Widget.Toolbar toolbar;
        private int requestCode = 1;

        private ConfigurationManager configurationManager;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.LockScreenSettings);
            BindViews();
            //TODO: DO i really want this?:
            //configurationManager = new ConfigurationManager(GetSharedPreferences("livedisplayconfig", FileCreationMode.Private));
            FragmentManager.BeginTransaction().Replace(Resource.Id.content, new LiveDisplayPreferencesFragment()).Commit();

           
            //BindClickEvents();
            //RetrieveLockScreenConfiguration();
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (this.requestCode == requestCode && resultCode == Result.Ok && data != null)
            {
                Android.Net.Uri uri = data.Data;
                try
                {
                    
                    BackgroundFactory background = new BackgroundFactory();
                    background.SaveImagePath(uri);
                    background = null;

                }
                catch
                {

                }
            }
        }
        private void BindViews()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            }
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.lockscreenSettingsToolbar);
            SetSupportActionBar(toolbar);

            //swToggleClock = FindViewById<SwitchCompat>(Resource.Id.swToggleClock);
            //swToggleSystemIcons = FindViewById<SwitchCompat>(Resource.Id.swToggleSystemIcons);
            //swUseLockscreenNoNotifications = FindViewById<SwitchCompat>(Resource.Id.swUseLockscreenNoNotifications);
            //swDynamicWallpaper = FindViewById<SwitchCompat>(Resource.Id.swDynamicWallpaper);
            //btnChangeWallpaper = FindViewById<Button>(Resource.Id.btnChangeWallpaper);
        }
        private void BindClickEvents()
        {
            swToggleClock.CheckedChange += SwToggleClock_CheckedChange;
            swToggleSystemIcons.CheckedChange += SwToggleSystemIcons_CheckedChange;
            swUseLockscreenNoNotifications.CheckedChange += SwUseLockscreenNoNotifications_CheckedChange;
            swDynamicWallpaper.CheckedChange += SwDymamicWallpaper_CheckedChange;
            btnChangeWallpaper.Click += BtnChangeWallpaper_Click;
          
        }      
        private void UnbindClickEvents()
        {
            swToggleClock.CheckedChange -= SwToggleClock_CheckedChange;
            swToggleSystemIcons.CheckedChange -= SwToggleSystemIcons_CheckedChange;
            swUseLockscreenNoNotifications.CheckedChange -= SwUseLockscreenNoNotifications_CheckedChange;
            swDynamicWallpaper.CheckedChange -= SwDymamicWallpaper_CheckedChange;
            btnChangeWallpaper.Click -= BtnChangeWallpaper_Click;
        }
        private void SwToggleClock_CheckedChange(object sender, EventArgs e)
        {
            if (swToggleClock.Checked == true)
            {
                configurationManager.SaveAValue(ConfigurationParameters.hiddenclock, true);
            }
            else if (swToggleClock.Checked==false)
            {
                configurationManager.SaveAValue(ConfigurationParameters.hiddenclock, false);
            }
        }
        private void SwToggleSystemIcons_CheckedChange(object sender, EventArgs e)
        {
            if (swToggleSystemIcons.Checked == true)
            {
                configurationManager.SaveAValue(ConfigurationParameters.hiddensystemicons, true);
            }
            else if(swToggleSystemIcons.Checked==false)
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
            else if (swUseLockscreenNoNotifications.Checked==false)
            {
                configurationManager.SaveAValue(ConfigurationParameters.enabledlockscreennonotifications, false);
            }
        }
        private void SwDymamicWallpaper_CheckedChange(object sender, EventArgs e)
        {
            if (swDynamicWallpaper.Checked == true)
            {
                configurationManager.SaveAValue(ConfigurationParameters.dynamicwallpaperenabled, true);
            }
            else if (swDynamicWallpaper.Checked==false)
            {
                configurationManager.SaveAValue(ConfigurationParameters.dynamicwallpaperenabled, false);
            }
        }
        private void BtnChangeWallpaper_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, "Pick image"), requestCode);
        }
        private void RetrieveLockScreenConfiguration()
        {
            
            swToggleClock.Checked = configurationManager.RetrieveAValue(ConfigurationParameters.hiddenclock) == true ? true : false;
            swToggleSystemIcons.Checked = configurationManager.RetrieveAValue(ConfigurationParameters.hiddensystemicons) == true ? true : false;
            swUseLockscreenNoNotifications.Checked = configurationManager.RetrieveAValue(ConfigurationParameters.enabledlockscreennonotifications) == true ? true : false;
            swDynamicWallpaper.Checked = configurationManager.RetrieveAValue(ConfigurationParameters.dynamicwallpaperenabled) == true ? true : false;

        }
        //Implement OnDestroy and kill all the references.
    }
}
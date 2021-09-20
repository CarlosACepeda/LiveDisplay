using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using LiveDisplay.Activities;
using LiveDisplay.Misc;
using LiveDisplay.Services;
using LiveDisplay.Services.Awake;
using System;

namespace LiveDisplay.Fragments.Preferences
{
    public class AwakeSettingsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private bool isSleepstarttimesetted = false;
        private ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        private TimePickerDialog startTimeDialog;
        private TimePickerDialog finishTimeDialog;
        private string digitalWellbeingPackageName = "com.google.android.apps.wellbeing";

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnResume()
        {
            sharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
            base.OnResume();
        }

        public override void OnPause()
        {
            sharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
            base.OnPause();
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.awake_prefs);
            SwitchPreference listenfordevicemotion = FindPreference("listenfordevicemotion?") as SwitchPreference;
            if (listenfordevicemotion.Checked == false)
            {
                ToggleAwakeSettingsItems(false);
            }
            else
            {
                ToggleAwakeSettingsItems(true);
            }
        }

        private void ToggleAwakeSettingsItems(bool enableItems)
        {
            Preference turnonusermovement = FindPreference("turnonusermovement?");
            Preference awakecausesblackwallpaper = FindPreference("awakecausesblackwallpaper?");
            Preference inactivehourssettingspreference = FindPreference("inactivetimesettings");
            //Preference syncwithdigitalwellbeing = FindPreference("syncwithdigitalwellbeing?");

            inactivehourssettingspreference.Enabled = enableItems;
            inactivehourssettingspreference.Selectable = enableItems;

            awakecausesblackwallpaper.Enabled = enableItems;
            awakecausesblackwallpaper.Selectable = enableItems;

            //syncwithdigitalwellbeing.Enabled = enableItems;
            //syncwithdigitalwellbeing.Selectable = enableItems;

            inactivehourssettingspreference.PreferenceClick += Inactivehourssettingspreference_PreferenceClick;

            if (new ConfigurationManager(AppPreferences.Default).RetrieveAValue(ConfigurationParameters.ListenForDeviceMotion) == false)
            {
                turnonusermovement.Enabled = false;
                turnonusermovement.Selectable = false;
            }
            else
            {
                turnonusermovement.Enabled = enableItems;
                turnonusermovement.Selectable = enableItems;
                if (enableItems == false)
                {
                    //User disabled Device Motion, so the service should be stopped as well.
                    AwakeHelper.ToggleStartStopAwakeService(false);
                }
            }
        }

        private void Blacklistpreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(BlacklistActivity)));
            StartActivity(intent);
        }

        private void Inactivehourssettingspreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            startTimeDialog = new TimePickerDialog(Activity, PreferencesFragmentCompat_starttimepicked, DateTime.Now.Hour, DateTime.Now.Minute, false);
            if (AwakeHelper.UserHasSetAwakeHours())
            {
                int start = int.Parse(new ConfigurationManager(AppPreferences.Default).RetrieveAValue(ConfigurationParameters.StartSleepTime, "-1"));
                startTimeDialog.SetMessage("Start hour: "); //here it goes the set start hour, (but in a user readable way)
            }
            else
            {
                startTimeDialog.SetMessage("Start hour:");
            }
            startTimeDialog.Show();
        }

        private void PreferencesFragmentCompat_starttimepicked(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            startTimeDialog.Dismiss();
            isSleepstarttimesetted = true;
            ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
            configurationManager.SaveAValue(ConfigurationParameters.StartSleepTime, string.Concat(e.HourOfDay.ToString() + e.Minute.ToString()));

            int end = int.Parse(new ConfigurationManager(AppPreferences.Default).RetrieveAValue(ConfigurationParameters.FinishSleepTime, "-1"));
            finishTimeDialog = new TimePickerDialog(Activity, PreferencesFragmentCompat_finishtimepicked, DateTime.Now.Hour, DateTime.Now.Minute, false);
            if (AwakeHelper.UserHasSetAwakeHours())
            {
                finishTimeDialog.SetMessage("Finish hour: "); //here it goes the set finish hour, (but in a user readable way)
            }
            else
            {
                finishTimeDialog.SetMessage("Finish hour:");
            }
            finishTimeDialog.Show();
        }

        private void PreferencesFragmentCompat_finishtimepicked(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
            if (isSleepstarttimesetted)
            {
                configurationManager.SaveAValue(ConfigurationParameters.FinishSleepTime, string.Concat(e.HourOfDay.ToString() + e.Minute.ToString()));
                isSleepstarttimesetted = true;
            }
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            switch (key)
            {
                case ConfigurationParameters.DoubleTapOnTopActionBehavior:
                    Preference doubletaptopbehavior = FindPreference("doubletapontoppactionbehavior");
                    switch (sharedPreferences.GetString(ConfigurationParameters.DoubleTapOnTopActionBehavior, "0"))
                    {
                        case "0":
                            doubletaptopbehavior.SetSummary(Resource.String.doubletaptopactiondesc);
                            break;

                        case "1":
                            doubletaptopbehavior.SetSummary(Resource.String.doubletaptopactioninverteddesc);
                            break;
                    }
                    break;

                case ConfigurationParameters.ListenForDeviceMotion:
                    Preference turnonusermovement = FindPreference("turnonusermovement?");
                    switch (sharedPreferences.GetBoolean(ConfigurationParameters.ListenForDeviceMotion, false))
                    {
                        case true:
                            AwakeHelper.ToggleStartStopAwakeService(true);
                            ToggleAwakeSettingsItems(true);
                            turnonusermovement.Enabled = true;
                            turnonusermovement.Selectable = true;

                            break;

                        case false:
                            AwakeHelper.ToggleStartStopAwakeService(false);
                            ToggleAwakeSettingsItems(false);
                            turnonusermovement.Enabled = false;
                            turnonusermovement.Selectable = false;
                            break;
                    }
                    break;
                case ConfigurationParameters.SyncWithDigitalWellbeing:
                    if(PackageUtils.GetTheAppName(digitalWellbeingPackageName)== null)
                    {
                        Activity.RunOnUiThread(() => Toast.MakeText(Activity, Resource.String.youneeddigitalwellbeingapp, ToastLength.Long).Show());
                    }

                    if (Checkers.IsNotificationListenerEnabled()==false)
                    {
                        SwitchPreference syncwithdigitalwellbeing = FindPreference("syncwithdigitalwellbeing?") as SwitchPreference;
                        //new ConfigurationManager(AppPreferences.Default).SaveAValue(ConfigurationParameters.SyncWithDigitalWellbeing, false);
                        syncwithdigitalwellbeing.Checked = false;
                        Activity.RunOnUiThread(() => Toast.MakeText(Activity, Resource.String.unabletoenablesyncwbedmode, ToastLength.Long).Show());
                    }
                    break;
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}
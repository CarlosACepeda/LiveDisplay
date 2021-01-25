using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.Preference;
using LiveDisplay.Activities;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Awake;
using System;

namespace LiveDisplay.Fragments.Preferences
{
    public class AwakeSettingsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private bool isSleepstarttimesetted = false;
        private ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        private TimePickerDialog startTimeDialog;
        private TimePickerDialog finishTimeDialog;

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
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.awake_prefs, false);
            SwitchPreference enableawake = FindPreference("enableawake?") as SwitchPreference;
            if (enableawake.Checked == false)
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
            Preference listendevicemotion = FindPreference("listenfordevicemotion?");
            Preference turnonnewnotification = FindPreference("turnonnewnotification?");
            Preference turnonusermovement = FindPreference("turnonusermovement?");
            Preference doubletapontopactionbehavior = FindPreference("doubletapontoppactionbehavior");
            Preference startlockscreendelaytime = FindPreference("startlockscreendelaytime");
            Preference turnoffscreendelaytime = FindPreference("turnoffscreendelaytime");
            Preference awakecausesblackwallpaper = FindPreference("awakecausesblackwallpaper?");
            Preference inactivehourssettingspreference = FindPreference("inactivetimesettings");

            listendevicemotion.Enabled = enableItems;
            listendevicemotion.Selectable = enableItems;

            inactivehourssettingspreference.Enabled = enableItems;
            inactivehourssettingspreference.Selectable = enableItems;

            turnonnewnotification.Enabled = enableItems;
            turnonnewnotification.Selectable = enableItems;

            awakecausesblackwallpaper.Enabled = enableItems;
            awakecausesblackwallpaper.Selectable = enableItems;

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
            doubletapontopactionbehavior.Enabled = enableItems;
            doubletapontopactionbehavior.Selectable = enableItems;

            startlockscreendelaytime.Enabled = enableItems;
            startlockscreendelaytime.Selectable = enableItems;

            turnoffscreendelaytime.Enabled = enableItems;
            turnoffscreendelaytime.Selectable = enableItems;
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

                case ConfigurationParameters.EnableAwakeService:
                    switch (sharedPreferences.GetBoolean(ConfigurationParameters.EnableAwakeService, false))
                    {
                        case true:
                            if (AwakeHelper.GetAwakeStatus() == AwakeStatus.CompletelyDisabled)
                            {
                                //What should go here
                            }
                            else
                            {
                                ToggleAwakeSettingsItems(true);
                            }
                            break;

                        case false:
                            ToggleAwakeSettingsItems(false);
                            break;
                    }

                    break;

                case ConfigurationParameters.ListenForDeviceMotion:
                    Preference turnonusermovement = FindPreference("turnonusermovement?");
                    switch (sharedPreferences.GetBoolean(ConfigurationParameters.ListenForDeviceMotion, false))
                    {
                        case true:
                            AwakeHelper.ToggleStartStopAwakeService(true);
                            turnonusermovement.Enabled = true;
                            turnonusermovement.Selectable = true;

                            break;

                        case false:
                            AwakeHelper.ToggleStartStopAwakeService(false);
                            turnonusermovement.Enabled = false;
                            turnonusermovement.Selectable = false;
                            break;
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
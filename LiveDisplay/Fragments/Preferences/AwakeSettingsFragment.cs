using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using LiveDisplay.Activities;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;
using LiveDisplay.Servicios.Awake;
using System;

namespace LiveDisplay.Fragments.Preferences
{
    public class  AwakeSettingsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private bool isSleepstarttimesetted = false;
        private ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);


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
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.awake_prefs, true);
            SwitchPreference enableawake = FindPreference("enableawake?") as SwitchPreference;
            if (enableawake.Checked == false)
            {
                ToggleAwakeSettingsItems(false);
            }
            else 
            {
                if (AwakeHelper.GetAwakeStatus() == AwakeStatus.CompletelyDisabled)
                {
                    Inactivehourssettingspreference_PreferenceClick(null, null);
                    ToggleAwakeSettingsItems(true);
                }

            }
        }



        void ToggleAwakeSettingsItems(bool enableItems)
        {
            Preference listendevicemotion = FindPreference("listenfordevicemotion?");
            Preference inactivehourssettingspreference = FindPreference("inactivetimesettings");
            Preference turnonnewnotification = FindPreference("turnonnewnotification?");
            Preference turnonusermovement = FindPreference("turnonusermovement?");
            Preference doubletapontopactionbehavior = FindPreference("doubletapontoppactionbehavior");
            Preference startlockscreendelaytime = FindPreference("startlockscreendelaytime");
            Preference turnoffscreendelaytime = FindPreference("turnoffscreendelaytime");

            listendevicemotion.Enabled = enableItems;
            listendevicemotion.Selectable = enableItems;

            inactivehourssettingspreference.Enabled = enableItems;
            inactivehourssettingspreference.Selectable = enableItems;

            turnonnewnotification.Enabled = enableItems;
            turnonnewnotification.Selectable = enableItems;

            if (new ConfigurationManager(AppPreferences.Default).RetrieveAValue(ConfigurationParameters.ListenForDeviceMotion) == false)
            {
                turnonusermovement.Enabled = false;
                turnonusermovement.Selectable = false;
            }
            else
            {
                turnonusermovement.Enabled = enableItems;
                turnonusermovement.Selectable = enableItems;
            }
            doubletapontopactionbehavior.Enabled = enableItems;
            doubletapontopactionbehavior.Selectable = enableItems;

            startlockscreendelaytime.Enabled = enableItems;
            startlockscreendelaytime.Selectable = enableItems;

            turnoffscreendelaytime.Enabled = enableItems;
            turnoffscreendelaytime.Selectable = enableItems;

            inactivehourssettingspreference.PreferenceClick += Inactivehourssettingspreference_PreferenceClick;

        }

        private void Blacklistpreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(BlacklistActivity)));
            StartActivity(intent);
        }

        private void Inactivehourssettingspreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            using (TimePickerDialog datePickerDialog = new TimePickerDialog(Activity, PreferencesFragmentCompat_timepicked, DateTime.Now.Hour, DateTime.Now.Minute, false))
            {
                if (!isSleepstarttimesetted)
                {
                    Toast.MakeText(Activity, "Set the Start hour", ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(Activity, "Set the finish hour", ToastLength.Long).Show();
                }
                //datePickerDialog.Create();
                datePickerDialog.Show();
            }
        }

        private void PreferencesFragmentCompat_timepicked(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            //Simple trick to save two different values using the same timepicker.

            ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);
            if (isSleepstarttimesetted)
            {
                configurationManager.SaveAValue(ConfigurationParameters.FinishSleepTime, string.Concat(e.HourOfDay.ToString() + e.Minute.ToString()));
            }
            else
            {
                configurationManager.SaveAValue(ConfigurationParameters.StartSleepTime, string.Concat(e.HourOfDay.ToString() + e.Minute.ToString()));
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
                                Inactivehourssettingspreference_PreferenceClick(null, null);
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
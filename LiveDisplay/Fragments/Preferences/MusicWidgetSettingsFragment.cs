﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using LiveDisplay.Misc;
using LiveDisplay.Services;

namespace LiveDisplay.Fragments.Preferences
{
    public class MusicWidgetSettingsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        private ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default);

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.music_widget_prefs);
            if (Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch) //In kitkat you can only use the RemoteController
                                                                      //thats why you can't choose a music widget control method.
            {
                Preference musiccontrolmethod = FindPreference("musicwidgetcontrolmethod"); //Preference that's disabled by default.
                musiccontrolmethod.Selectable = true;
                musiccontrolmethod.Enabled = true;
            }
            string interval = configurationManager.RetrieveAValue(ConfigurationParameters.MusicWidgetMethod, "1"); //1 is the default value, music_widget_prefs.xml
            ListPreference musicWidgetControl = FindPreference("musicwidgetcontrolmethod") as ListPreference;
            musicWidgetControl.Value = interval;
            musicWidgetControl.Summary = musicWidgetControl.Entry;

            sharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
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

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            switch (key)
            {
                case ConfigurationParameters.MusicWidgetMethod:
                    Preference musiccontrolmethod = FindPreference("musicwidgetcontrolmethod");
                    switch (sharedPreferences.GetString(ConfigurationParameters.MusicWidgetMethod, "0"))
                    {
                        case "0":
                            musiccontrolmethod.SetSummary(Resource.String.m_widgetcontrolmethodsession);
                            if (!Checkers.IsNotificationListenerEnabled())
                            {
                                Toast.MakeText(Activity, Resource.String.itneedsnotificationaccesstowork, ToastLength.Long).Show();
                                configurationManager.SaveAValue(ConfigurationParameters.MusicWidgetMethod, "1");
                            }
                            else {
                                NotificationSlave.NotificationSlaveInstance().ToggleMediaSessionsListener(true);
                            }
                            break;

                        case "1":
                            musiccontrolmethod.SetSummary(Resource.String.m_widgetcontrolmethodnotification);
                            NotificationSlave.NotificationSlaveInstance().ToggleMediaSessionsListener(false);
                            break;
                    }
                    break;

                case ConfigurationParameters.HideNotificationWhenItsMediaPlaying:
                    switch (sharedPreferences.GetBoolean(ConfigurationParameters.HideNotificationWhenItsMediaPlaying, false))
                    {
                        case true:
                            SwitchPreference launchnotification = (SwitchPreference)FindPreference("launchnotification?");

                            launchnotification.Checked = false;
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
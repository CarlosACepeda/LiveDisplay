using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.Preference;
using LiveDisplay.Misc;

namespace LiveDisplay.Fragments.Preferences
{
    public class MusicWidgetSettingsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.music_widget_prefs);
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.music_widget_prefs, true);
            if (Build.VERSION.SdkInt > BuildVersionCodes.KitkatWatch) //In kitkat you can only use the RemoteController
                //thats why you can't choose a music widget control method.
            {
                Preference musiccontrolmethod = FindPreference("musicwidgetcontrolmethod"); //Preference that's disabled by default.
                musiccontrolmethod.Selectable = true; 
                musiccontrolmethod.Enabled = true;
            }

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
                    Preference hidenotifwhenmediaplaying = FindPreference("hidenotificationwhenmediaplaying?");
                    switch (sharedPreferences.GetString(ConfigurationParameters.MusicWidgetMethod, "0"))
                    {
                        case "0":
                            musiccontrolmethod.SetSummary(Resource.String.m_widgetcontrolmethodsession);
                            //hidenotifwhenmediaplaying.Selectable = false;
                            //hidenotifwhenmediaplaying.Enabled = false;
                            break;

                        case "1":
                            musiccontrolmethod.SetSummary(Resource.String.m_widgetcontrolmethodnotification);
                            //hidenotifwhenmediaplaying.Selectable = true;
                            //hidenotifwhenmediaplaying.Enabled = true;
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
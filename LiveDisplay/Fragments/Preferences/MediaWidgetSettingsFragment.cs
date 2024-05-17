using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.Preference;
using LiveDisplay.Misc;

namespace LiveDisplay.Fragments.Preferences
{
    public class MediaWidgetSettingsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.media_widget_prefs);
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.media_widget_prefs, true);

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
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}
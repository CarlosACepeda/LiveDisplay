using Android.App;
using Android.OS;
using Android.Views;
using AndroidX.Preference;

namespace LiveDisplay.Fragments.Preferences
{
    public class MusicWidgetSettingsFragment : PreferenceFragmentCompat
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.music_widget_prefs);
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.music_widget_prefs, true);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}
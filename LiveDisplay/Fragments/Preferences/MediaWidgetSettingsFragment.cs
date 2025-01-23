using Android.App;
using Android.OS;
using Android.Views;
using AndroidX.Preference;
using LiveDisplay.Misc;
using LiveDisplay.Services.Media;
using System.Collections.Generic;
using System.Linq;

namespace LiveDisplay.Fragments.Preferences
{
    public class MediaWidgetSettingsFragment : PreferenceFragmentCompat
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.media_widget_prefs);
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.media_widget_prefs, true);

            List<string> entries = new List<string>();
            var blockedSessions = RecentSessionsProvider.GetInstance().GetBlockedSessions();
            var allowedSessions = RecentSessionsProvider.GetInstance().GetSavedSessions();

            var allSessions = allowedSessions.Except(blockedSessions).Concat(blockedSessions.Except(allowedSessions));

            foreach (var session in allSessions)
            {
                entries.Add(PackageUtils.GetTheAppName(session));
            }

            MultiSelectListPreference multiSelectPref = new MultiSelectListPreference(Application.Context)
            {
                Key = ConfigurationParameters.BlockedMediaSessions,
                Title = Resources.GetString(Resource.String.blocked_media_sessions),
                Summary= Resources.GetString(Resource.String.blocked_media_sessions_summary),
                DialogTitle= Resources.GetString(Resource.String.blocked_media_sessions)
            };
            multiSelectPref.SetEntries(entries.ToArray());
            multiSelectPref.SetEntryValues(allSessions.ToArray());
            PreferenceScreen.AddPreference(multiSelectPref);

        }
        public override void OnResume()
        {
            base.OnResume();
        }
        public override void OnPause()
        {
            base.OnPause();
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}
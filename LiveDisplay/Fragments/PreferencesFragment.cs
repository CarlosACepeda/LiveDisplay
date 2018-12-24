using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using LiveDisplay.Activities;
using LiveDisplay.Factories;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;

namespace LiveDisplay.Fragments
{
    public class PreferencesFragment : PreferenceFragment, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AddPreferencesFromResource(Resource.Xml.prefs);
            sharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
            Preference wallpapersettingspreference = FindPreference("wallpapersettings");
            Preference blacklistpreference = FindPreference("blacklist");
            wallpapersettingspreference.PreferenceClick += WallpaperSettingsPreference_PreferenceClick;
            blacklistpreference.PreferenceClick += Blacklistpreference_PreferenceClick;
        }

        private void Blacklistpreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(BlacklistActivity)));
            StartActivity(intent);
        }

        private void WallpaperSettingsPreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            Intent intent = new Intent(Application.Context, Java.Lang.Class.FromType(typeof(BackgroundSettingsActivity)));
            StartActivity(intent);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override void OnPause()
        {
            base.OnPause();
            sharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1 && resultCode == Result.Ok && data != null)
            {
                Android.Net.Uri uri = data.Data;
                try
                {
                    BackgroundFactory background = new BackgroundFactory();
                    background.SaveImagePath(uri);
                    background = null;
                    Log.Info("tag", "Path sent to BackgroundFactory");
                }
                catch
                {
                }
            }
            else
            {
                Log.Info("tag", "Data was null");
                using (ConfigurationManager configurationManager = new ConfigurationManager(sharedPreferences))
                {
                    configurationManager.SaveAValue(ConfigurationParameters.changewallpaper, "0");
                }
            }
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (key == ConfigurationParameters.changewallpaper)
            {
                //2 means 'User will pick custom wallpaper' 0 means black.
                if (sharedPreferences.GetString(ConfigurationParameters.changewallpaper, "0") == "2")
                {
                    using (Intent intent = new Intent())
                    {
                        intent.SetType("image/*");
                        intent.SetAction(Intent.ActionGetContent);
                        //here 1 means the request code.
                        StartActivityForResult(Intent.CreateChooser(intent, "Pick image"), 1);
                    }
                }
            }
        }
    }
}
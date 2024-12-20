namespace LiveDisplay.Activities
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Widget;
    using AndroidX.AppCompat.App;
    using AndroidX.Preference;
    using LiveDisplay.Fragments;
    using LiveDisplay.Fragments.Preferences;

    [Activity(Label = "@string/settings")]
    public class SettingsActivity : AppCompatActivity, PreferenceFragmentCompat.IOnPreferenceStartFragmentCallback
    {
        private AndroidX.AppCompat.Widget.Toolbar toolbar;
        AndroidX.Fragment.App.Fragment fragment = null;
        AndroidX.Fragment.App.Fragment preferencesFragment = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Settings);
            using (toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar))
            {
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDefaultDisplayHomeAsUpEnabled(true);
            }
            preferencesFragment = new PreferencesFragment();
        }
        protected override void OnDestroy()
        {
            fragment = null;
            preferencesFragment = null;
            base.OnDestroy();
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            SupportFragmentManager.BeginTransaction().Add
                (Resource.Id.content, preferencesFragment).Commit();
        }

        public bool OnPreferenceStartFragment(PreferenceFragmentCompat caller, Preference pref)
        {
            int fragmentId = -1;
            string activityQualifiedName = string.Empty;
            //Switch: a Workaround, there's not possible way to get the Qualified name of the Fragment to Start
            //in Xamarin Android.

            //Check first if what we have to start is a fragment replace or start a new activity
            //I've developed a custom convention for this.
            //if the name of the fragment class file contains 'Fragment' then it should do the replace fragment operation
            //if contains 'Activity' then it should start an activity.
            if (pref.Fragment.Contains("Fragment"))
            {
                switch (pref.Fragment)
                {
                    case "MediaWidgetSettingsFragment":
                        fragment = new MediaWidgetSettingsFragment();
                        fragmentId = 0;
                        break;
                    case "AboutFragment":
                        fragment = new AboutFragment();

                        fragmentId = 1;
                        break;
                    case "AppearanceSettingsFragment":
                        fragment = new LockScreenSettingsFragment();
                        fragmentId = 2;
                        break;
                    case "MediaControlsProviderServiceSettingsFragment":
                        fragment = new MediaControlsProviderServiceSettingsFragment();
                        fragmentId = 3;
                        break;
                    case "WeatherSettingsFragment":
                        fragment = new WeatherSettingsFragment();
                        fragmentId = 4;
                        break;
                    default:
                        break;
                }

                SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.content, fragment, fragmentId.ToString())
                        .AddToBackStack(null)
                        .SetTransition(AndroidX.Fragment.App.FragmentTransaction.TransitFragmentMatchActivityOpen)
                        .Commit();

            }
            else if (pref.Fragment.Contains("Activity"))
            {
                switch (pref.Fragment)
                {
                    case "WeatherSettingsActivity":
                        activityQualifiedName = Java.Lang.Class.FromType(typeof(WeatherSettingsActivity)).Name;
                        break;
                }
                using Intent intent = new Intent(Application.Context, Java.Lang.Class.ForName(activityQualifiedName));
                StartActivity(intent);
            }
            return true;
        }
    }
}
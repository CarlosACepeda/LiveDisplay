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

    [Activity(Label = "@string/settings", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
    public class SettingsActivity : AppCompatActivity, PreferenceFragmentCompat.IOnPreferenceStartFragmentCallback
    {
        private AndroidX.AppCompat.Widget.Toolbar toolbar;

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
            if (Build.VERSION.SdkInt > BuildVersionCodes.Kitkat)
            {
                Bundle remoteInput = RemoteInput.GetResultsFromIntent(Intent);
                if (remoteInput != null)
                {
                    string response = remoteInput.GetCharSequence("test1");

                    Toast.MakeText(this, "The response is: " + response, ToastLength.Long).Show();
                }
            }
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content, new PreferencesFragment()).Commit();
        }

        public bool OnPreferenceStartFragment(PreferenceFragmentCompat caller, Preference pref)
        {
            string fragmentQualifiedName = string.Empty;
            //Switch: a Workaround, there's not possible way to get the fragment's qualified name to Start
            //in Xamarin Android.

            //Check first if what we have to start is a fragment replace or start a new activity
            //I've developed a custom convention for this.
            //if the name of the fragment class file contains 'Fragment' then it should do the replace fragment operation
            //if contains 'Activity' then it should start an activity.
            if (pref.Fragment.Contains("Fragment"))
            {
                switch (pref.Fragment)
                {
                    case "LockScreenSettingsFragment":
                        fragmentQualifiedName = Java.Lang.Class.FromType(typeof(LockScreenSettingsFragment)).Name;
                        break;
                    case "NotificationSettingsFragment":
                        fragmentQualifiedName = Java.Lang.Class.FromType(typeof(NotificationSettingsFragment)).Name;
                        break;
                    case "AwakeSettingsFragment":
                        fragmentQualifiedName = Java.Lang.Class.FromType(typeof(AwakeSettingsFragment)).Name;
                        break;
                    case "MusicWidgetSettingsFragment":
                        fragmentQualifiedName = Java.Lang.Class.FromType(typeof(MusicWidgetSettingsFragment)).Name;
                        break;
                    case "WeatherSettingsFragment":
                        fragmentQualifiedName = Java.Lang.Class.FromType(typeof(WeatherSettingsFragment)).Name;
                        break; 
                    case "AboutFragment":
                        fragmentQualifiedName = Java.Lang.Class.FromType(typeof(AboutFragment)).Name;
                        break;
                    default:
                        break;
                }
                // Instantiate the new Fragment
                var args = pref.Extras;
                var fragment = SupportFragmentManager.FragmentFactory.Instantiate(
                    ClassLoader,
                    fragmentQualifiedName); //Normally it should be 'pref.Fragment'
                fragment.Arguments = args;
                fragment.SetTargetFragment(caller, 0);
                // Replace the existing Fragment with the new Fragment
                SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.content, fragment)
                        .AddToBackStack(null)
                        .Commit();
            }            
            return true;
        }
    }
}
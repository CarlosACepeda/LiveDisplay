using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;

namespace LiveDisplay.Fragments.Preferences
{
    public class AboutFragment : PreferenceFragmentCompat
    {
        public const string ContactEmailUri = "mailto:carlosalt5126@hotmail.es";
        public const string RepoUrl = "https://github.com/CarlosACepeda/LiveDisplay/";


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.about);
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.about, false);
            Preference githubprojectpreference = FindPreference("contributetoproject");


            Preference askfortranslations = FindPreference("askfortranslations");
            githubprojectpreference.PreferenceClick += Githubprojectpreference_PreferenceClick;
            askfortranslations.PreferenceClick += Askfortranslations_PreferenceClick; ;
        }

        private void Askfortranslations_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            Intent intent = new Intent(Intent.ActionView);
            intent.SetData(Android.Net.Uri.Parse(ContactEmailUri));
            Bundle extras = new Bundle();
            extras.PutString(Intent.ExtraSubject, Resources.GetString(Resource.String.email_subject));
            extras.PutString(Intent.ExtraText, Resources.GetString(Resource.String.email_text));
            intent.PutExtras(extras);
            try
            {
                StartActivity(intent);
            }
            catch(ActivityNotFoundException)
            {
                Activity.RunOnUiThread(() =>
                Toast.MakeText(Activity, Resource.String.no_activity_found_to_handle_email, ToastLength.Long).Show()
                );
            }

        }

        private void Githubprojectpreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            Intent intent = new Intent(Intent.ActionView);
            intent.SetData(Android.Net.Uri.Parse(RepoUrl));
            StartActivity(intent);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}
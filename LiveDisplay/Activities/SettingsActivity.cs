using Android.App;
using Android.OS;
using Android.Support.V7.App;
using LiveDisplay.Fragments;

namespace LiveDisplay.Activities
{
    [Activity(Label = "@string/settings", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
    public class SettingsActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Intent != null)
            {
                Bundle bundle = RemoteInput.GetResultsFromIntent(Intent);
                if (bundle != null)
                {
                    var lmao = bundle.GetCharSequence("test");
                }
            }

            // Create your application here
            SetContentView(Resource.Layout.Settings);
            using (toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar))
            {
                SetSupportActionBar(toolbar);
            }

            FragmentManager.BeginTransaction().Replace(Resource.Id.content, new PreferencesFragment()).Commit();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
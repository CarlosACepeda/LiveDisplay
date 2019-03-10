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

            // Create your application here
            SetContentView(Resource.Layout.Settings);
            using (toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar))
            {
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDefaultDisplayHomeAsUpEnabled(true);
            }
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            FragmentManager.BeginTransaction().Replace(Resource.Id.content, new PreferencesFragmentCompat()).Commit();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
namespace LiveDisplay.Activities
{
    using Android.App;
    using Android.OS;
    using Android.Support.V7.App;
    using Android.Widget;
    using LiveDisplay.Fragments;

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
            Bundle remoteInput = RemoteInput.GetResultsFromIntent(Intent);
            if (remoteInput != null)
            {
                string response = remoteInput.GetCharSequence("test1");

                Toast.MakeText(this, "The response is: " + response, ToastLength.Long).Show();
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
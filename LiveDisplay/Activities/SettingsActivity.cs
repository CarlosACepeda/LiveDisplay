namespace LiveDisplay.Activities
{
    using Android.App;
    using Android.OS;
    using AndroidX.AppCompat.App;
    using Android.Widget;
    using LiveDisplay.Fragments;
    using AndroidX.Preference;

    [Activity(Label = "@string/settings", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
    public class SettingsActivity : AppCompatActivity
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

    }
}
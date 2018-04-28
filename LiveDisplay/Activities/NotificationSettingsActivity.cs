using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;

namespace LiveDisplay.Activities
{
    [Activity(Label = "@string/notificationsettings")]
    public class NotificationSettingsActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.NotificationSettings);
            BindViews();
            // Create your application here
        }

        private void BindViews()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            }
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.notificationSettingsToolbar);
            SetSupportActionBar(toolbar);
        }
    }
}